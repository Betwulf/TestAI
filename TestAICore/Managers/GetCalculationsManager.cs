using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using TestAICore.Repositories;
using TestAICore.DataObjects;

namespace TestAICore.Managers
{
    public class GetCalculationsManager
    {
        private readonly int daysOfFutureScore;

        private IDocSource mDocDB;
        public GetCalculationsManager(IDocSource aDocDB)
        {
            string stringFutureWindow = ConfigurationManager.AppSettings["calc-future-window"];
            daysOfFutureScore = Int32.Parse(stringFutureWindow);
            mDocDB = aDocDB;
        }


        public async Task<CalcDataSeries> GetDaysCalculations(string aUniverseName, Action<string> updateMessage, Predicate<Constituent> aPredicate, DateTime aDate)
        {
            var uMan = new GetUniverseManager(mDocDB);
            var retVal = new CalcDataSeries();
            var tickerlist = await uMan.GetUniverse(aUniverseName, updateMessage);
            foreach (var ticker in tickerlist.Constituents.FindAll(aPredicate))
            {
                var calcedDataSeries = mDocDB.CollectionCalcData.GetById(ticker.Ticker);
                if (calcedDataSeries == null)
                {
                    updateMessage("* Missing Data for: " + ticker.Ticker);
                    continue;
                }
                var item = calcedDataSeries.CalcDataList.Find(t => t.Date == aDate);
                if (item != null) retVal.CalcDataList.Add(item);
            }
            return retVal;
        }



        public async Task<bool> CalculateTrainingSignal(string aUniverseName, Action<string> updateMessage)
        {
            updateMessage($"Begin CalculateTrainingSignal. daysOfFutureScore = {daysOfFutureScore}");
            var uMan = new GetUniverseManager(mDocDB);
            var tickerlist = await uMan.GetUniverse(aUniverseName, updateMessage);
            var trainingArray = new List<CalcDataWithTrainingSeries>();
            foreach (var ticker in tickerlist.Constituents)
            {
                updateMessage("Training Data for: " + ticker.Ticker);
                var trainingAlreadyExists = mDocDB.CollectionCalcDataWithTraining.GetById(ticker.Ticker);
                if (trainingAlreadyExists != null) { updateMessage("* Training Data already exists for: " + ticker.Ticker); continue; }
                var calcedDataSeries = mDocDB.CollectionCalcData.GetById(ticker.Ticker);
                if (calcedDataSeries == null) { updateMessage("* Missing Calc Data for: " + ticker.Ticker); continue; }

                var calcDataWithTrainingSeries = new CalcDataWithTrainingSeries();
                trainingArray.Add(calcDataWithTrainingSeries);
                calcDataWithTrainingSeries.Id = ticker.Ticker;
                calcedDataSeries.CalcDataList.Sort(); // Make sure Dates are in order
                for (int i = 1; i < calcedDataSeries.CalcDataList.Count; i++)
                {
                    var calcedData = calcedDataSeries.CalcDataList[i];
                    DateTime currentDate = calcedData.Date;
                    // Calc Signal
                    var fi = daysOfFutureScore + i;
                    if (fi < calcedDataSeries.CalcDataList.Count)
                    {
                        // Then we can calc a score
                        var futureDate = calcedDataSeries.CalcDataList[fi].Date;
                        var training = new CalcDataWithTraining();
                        training.SetFromCalcData(calcedData);

                        CalcData FutureSixtyDay = calcedDataSeries.CalcDataList[fi];
                        CalcData FutureNinthDay = calcedDataSeries.CalcDataList[9 + i];
                        CalcData FutureFifteenthDay = calcedDataSeries.CalcDataList[15 + i];
                        // SIGNAL MATH HERE !
                        training.BuySignal = ((FutureSixtyDay.SixtyDayReturn > 0.08M) && (FutureNinthDay.NineDayReturn > -0.05M)) ? 1 : 0;
                        training.SellSignal = (((FutureNinthDay.NineDayReturn < -0.05M) && (FutureSixtyDay.SixtyDayReturn < 0.04M)) || (FutureSixtyDay.StandardDeviation > 0.072)) ? 1 : 0;
                        calcDataWithTrainingSeries.CalcDataList.Add(training);
                    }
                }

                // Save calcDataWithTrainingSeries
                if (calcDataWithTrainingSeries.CalcDataList.Count > 0)
                {
                    await Task.Delay(5); // TODO: why is this here?
                    try
                    {
                        await mDocDB.CollectionCalcDataWithTraining.CreateAsync(calcDataWithTrainingSeries);
                    }
                    catch (Exception ex) { updateMessage(ex.Message); }
                }

            }
            // Aggregate Training Data
            var aggregatedTraining = new CalcDataWithTrainingSeries();
            foreach (var trainingSeries in trainingArray)
            {
                aggregatedTraining.CalcDataList.AddRange(trainingSeries.CalcDataList);
            }
            if (aggregatedTraining.CalcDataList.Count > 0)
            {
                try
                {
                    DateTime theTime = DateTime.Now;
                    var filename = @"C:\Temp\" + theTime.Hour.ToString() + theTime.Minute.ToString() + theTime.Second.ToString() + " Training Data.csv";
                    updateMessage("File: " + filename);
                    aggregatedTraining.CalcDataList.CreateCSVFromGenericList(filename);
                }
                catch (Exception ex)
                {
                    updateMessage(ex.Message);
                    throw;
                }
            }
            updateMessage("Training Data Done");
            return true;
        }




        public async Task<DateTime?> CreateCalculations(string aUniverseName, Action<string> updateMessage)
        {
            updateMessage($"Begin CreateCalculations. daysOfFutureScore = {daysOfFutureScore}");
            DateTime? latestDate = null;
            int aYearAgoInDays = 250;
            // Get the universe
            var uMan = new GetUniverseManager(mDocDB);
            var tickerlist = await uMan.GetUniverse(aUniverseName, updateMessage);
            Parallel.ForEach(tickerlist.Constituents, async (ticker) =>
            {
                // Check to see if doc is already generated 
                var c = mDocDB.CollectionCalcData.GetById(ticker.Ticker);
                var marketDataSeries = mDocDB.CollectionMarketData.GetById(ticker.Ticker);
                if (marketDataSeries == null) { updateMessage("* Missing data: " + ticker.Ticker); return; }
                CalcDataSeries calcDataSeries = new CalcDataSeries();
                if (c != null)
                {
                    updateMessage("Appending Calc Data for: " + ticker.Ticker);
                    calcDataSeries.CalcDataList = c.CalcDataList;
                }
                else
                {
                    updateMessage("Calcing Data for: " + ticker.Ticker);
                }
                calcDataSeries.SetFromMarketDataSeries(marketDataSeries);
                // First create adjustment vector
                marketDataSeries.MarketDataList.Sort(new MarketDataReverseComparer()); // Make sure Dates are in order from latest to oldest
                List<decimal> AdjustList = new List<decimal>();
                for (int i = 0; i < marketDataSeries.MarketDataList.Count; i++)
                {
                    var md = marketDataSeries.MarketDataList[i];
                    decimal AdjustValue = md.Close / md.AdjClose;
                    md.High = md.High / AdjustValue;
                    md.Low = md.Low / AdjustValue;
                    md.Open = md.Open / AdjustValue;
                    AdjustList.Add(AdjustValue);
                }

                marketDataSeries.MarketDataList.Sort(); // Make sure Dates are in order from old to new
                AdjustList.Reverse();
                for (int i = 0; i < marketDataSeries.MarketDataList.Count; i++)
                {
                    var md = marketDataSeries.MarketDataList[i]; // market data for the day
                    DateTime currDate = md.PriceDate;

                    // Update Latest DateTime to return
                    if (!latestDate.HasValue) latestDate = currDate;
                    if (currDate > latestDate.Value) latestDate = currDate;

                    // MAIN LOOP
                    if (i > aYearAgoInDays && calcDataSeries.CalcDataList.Find(x => x.Date == md.PriceDate) == null) // Need 1 years worth of back data for year high calcs
                    {
                        var mdYesterday = marketDataSeries.MarketDataList[i - 1];
                        var calcedData = new CalcData();
                        calcedData.SetFromMarketData(md);
                        calcedData.DailyReturn = (md.AdjClose / mdYesterday.AdjClose) - 1;
                        calcedData.ReturnOpen = ((md.Open / AdjustList[i]) / mdYesterday.AdjClose) - 1;
                        calcedData.ReturnHigh = ((md.High / AdjustList[i]) / mdYesterday.AdjClose) - 1;
                        calcedData.ReturnLow = ((md.Low / AdjustList[i]) / mdYesterday.AdjClose) - 1;

                        DateTime yearAgoDate = marketDataSeries.MarketDataList[i - aYearAgoInDays].PriceDate;
                        // Begin, we have enough history to create data
                        List<MarketData> yearList = marketDataSeries.MarketDataList.FindAll(x => x.PriceDate.CompareTo(currDate) <= 0 && x.PriceDate.CompareTo(yearAgoDate) >= 0);
                        // High/Low in mds was adjusted in the prior loop
                        var YearsHigh = yearList.Max(x => x.High);
                        var YearsLow = yearList.Min(x => x.Low);
                        calcedData.YearsHighPercent = (md.AdjClose - YearsLow) / (YearsHigh - YearsLow);
                        calcedData.YearsLowPercent = (YearsHigh - md.AdjClose) / (YearsHigh - YearsLow);

                        // Moving Averages & Returns
                        var NineDaysOld = marketDataSeries.MarketDataList[i - 9].PriceDate;
                        List<MarketData> NineDayList = marketDataSeries.MarketDataList.FindAll(x => x.PriceDate.CompareTo(currDate) <= 0 && x.PriceDate.CompareTo(NineDaysOld) > 0);
                        calcedData.MovingAvg9Percent = (NineDayList.Average(x => x.AdjClose) / md.AdjClose) - 1;
                        calcedData.NineDayReturn = (md.AdjClose / marketDataSeries.MarketDataList[i - 9].AdjClose) - 1;

                        var FifteenDaysOld = marketDataSeries.MarketDataList[i - 15].PriceDate;
                        List<MarketData> FifteenDayList = marketDataSeries.MarketDataList.FindAll(x => x.PriceDate.CompareTo(currDate) <= 0 && x.PriceDate.CompareTo(FifteenDaysOld) > 0);
                        calcedData.MovingAvg15Percent = (FifteenDayList.Average(x => x.AdjClose) / md.AdjClose) - 1;
                        calcedData.FifteenDayReturn = (md.AdjClose / marketDataSeries.MarketDataList[i - 15].AdjClose) - 1;

                        var ThirtyDaysOld = marketDataSeries.MarketDataList[i - 30].PriceDate;
                        List<MarketData> ThirtyDayList = marketDataSeries.MarketDataList.FindAll(x => x.PriceDate.CompareTo(currDate) <= 0 && x.PriceDate.CompareTo(ThirtyDaysOld) > 0);
                        calcedData.MovingAvg30Percent = (ThirtyDayList.Average(x => x.AdjClose) / md.AdjClose) - 1;
                        calcedData.ThirtyDayReturn = (md.AdjClose / marketDataSeries.MarketDataList[i - 30].AdjClose) - 1;

                        var SixtyDaysOld = marketDataSeries.MarketDataList[i - 60].PriceDate;
                        List<MarketData> SixtyDayList = marketDataSeries.MarketDataList.FindAll(x => x.PriceDate.CompareTo(currDate) <= 0 && x.PriceDate.CompareTo(SixtyDaysOld) > 0);
                        calcedData.MovingAvg60Percent = (SixtyDayList.Average(x => x.AdjClose) / md.AdjClose) - 1;
                        calcedData.SixtyDayReturn = (md.AdjClose / marketDataSeries.MarketDataList[i - 60].AdjClose) - 1;


                        // Std Dev
                        calcedData.StandardDeviation = ThirtyDayList.Select(x => x.AdjClose).StandardDeviation() / (double)md.AdjClose;

                        // Add calcedData to Series
                        calcDataSeries.CalcDataList.Add(calcedData);
                    }
                }

                try
                {
                    // Create CalcDataSeries Document in DocumentDB
                    calcDataSeries.CalcDataList.Sort();
                    await mDocDB.CollectionCalcData.CreateAsync(calcDataSeries);
                }
                catch (Exception ex)
                {
                    updateMessage($"Error in saving: {ex.Message}");
                    return;
                }

            });
            updateMessage("DONE CALCING Data.");
            return latestDate;
        }



    }
}
