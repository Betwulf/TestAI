using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using TestAICore.Repositories;
using TestAICore.DataObjects;
using TestAICore.DataSources;

namespace TestAICore.Managers
{
    public class GetMarketDataManager
    {
        private IDocDB mDocDB;
        public GetMarketDataManager(IDocDB aDocDB)
        {
            mDocDB = aDocDB;
        }


        private static int monthsBack = 30;




        /// <summary>
        /// Determines how to call DataCaller to get the appropriate amount of data
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="caller"></param>
        /// <param name="c"></param>
        /// <param name="updateMessage"></param>
        /// <returns>List of MarketData received from datacaller.</returns>
        private List<MarketData> GetMarketData(DateTime startDate, DateTime endDate, IDataCaller caller, Constituent c, Action<string> updateMessage)
        {
            var monthsBetween = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
            updateMessage("Getting data for: " + c.Ticker + " from " + startDate.ToShortDateString() + " to " + endDate.ToShortDateString());
            var series = new List<MarketData>();
            // Check to see if Yahoo is happy with size of history
            if (monthsBetween > 15)
            {
                DateTime middate = DateTime.Today.AddDays(-1).AddMonths(-15);
                var midseries = caller.GetHistoricalData(c.Ticker, startDate, middate, updateMessage);
                middate = middate.AddDays(1);
                series = caller.GetHistoricalData(c.Ticker, middate, endDate, updateMessage);
                series.AddRange(midseries);
            }
            else
            {
                series = caller.GetHistoricalData(c.Ticker, startDate, endDate, updateMessage);
            }
            return series;
        }



        public async Task<bool> CreateMarketData(IDataCaller caller, Constituent c, Action<string> updateMessage)
        {
            DateTime endDate = DateTime.Today.AddDays(-1);
            DateTime startDate = DateTime.Today.AddDays(-1).AddMonths(-monthsBack);

            var existingData = mDocDB.CollectionMarketData.GetById(c.Ticker);
            if (existingData != null)
            {
                existingData.MarketDataList.Sort(new MarketDataReverseComparer()); // Make sure Dates are in order
                var lastdate = existingData.MarketDataList.First().PriceDate;
                if (lastdate < endDate)
                { startDate = lastdate; }
                else
                {
                    updateMessage(" - Data is up to date for: " + c.Ticker + " from " + lastdate.ToShortDateString() + " to " + endDate.ToShortDateString());
                    return true;
                }
            }

            var series = GetMarketData(startDate, endDate, caller, c, updateMessage);

            if (existingData == null)
            {
                var mds = new MarketDataSeries() { MarketDataList = series };
                mds.SetFromConstituent(c);

                // Create MarketDataSeries Document in DocumentDB
                await mDocDB.CollectionMarketData.CreateAsync(mds);
                updateMessage("Saved Market data for: " + c.Ticker + " from " + startDate.ToShortDateString() + " to " + endDate.ToShortDateString());
            }
            else
            {
                existingData.StitchTogether(series);
                existingData.MarketDataList.Sort(); // Make sure Dates are in order
                await mDocDB.CollectionMarketData.UpdateAsync(existingData.Ticker, existingData);
            }
            return true;
        }




        public async Task<bool> GetMoreHistoricalMarketData(IDataCaller caller, Constituent c, int moreMonths, Action<string> updateMessage)
        {
            if (moreMonths > monthsBack)
            {
                updateMessage("Max months history is " + monthsBack + " cannot get more than that.");
                moreMonths = monthsBack;
            }
            // Get existing Data
            var existingData = mDocDB.CollectionMarketData.GetById(c.Ticker);
            if (existingData == null)
            {
                throw new Exception("Can't get more history - no file exists at all. Get initial data first.");
            }

            existingData.MarketDataList.Sort(); // Make sure Dates are in order
            // Figure out start and end dates
            DateTime endDate = existingData.MarketDataList.First().PriceDate.AddDays(1);
            DateTime startDate = endDate.AddMonths(-moreMonths);
            // Get history
            var series = GetMarketData(startDate, endDate, caller, c, updateMessage);
            //Save the lot
            existingData.StitchTogether(series, true);
            existingData.MarketDataList.Sort(); // Make sure Dates are in order
            await mDocDB.CollectionMarketData.UpdateAsync(existingData.Ticker, existingData);
            return true;
        }


        



        public async Task<bool> GetHistoryFromUniverse(string aUniverse, Action<string> updateMessage, bool moreHistory = false)
        {
            Universe u = mDocDB.CollectionUniverse.GetById(aUniverse);
            if (u == null) throw new Exception("Universe has no data: " + aUniverse);

            IDataCaller caller = null;
            string yahooToggle = ConfigurationManager.AppSettings["toggle-use-yahoo"];
            if (yahooToggle == "true")
            { caller = new YahooCaller(); updateMessage("Using Yahoo..."); }
            else
            { caller = new QuandlCaller(); updateMessage("Using Quandl..."); }

            foreach (var constituent in u.Constituents)
            {
                try
                {
                    if (moreHistory)
                    {
                        await GetMoreHistoricalMarketData(caller, constituent, 12, updateMessage);
                    }
                    else
                    {
                        await CreateMarketData(caller, constituent, updateMessage);
                    }
                }
                catch (Exception ex)
                {
                    updateMessage("Exception in Loop: " + constituent.Ticker + " : " + ex.Message);
                }
            }
            updateMessage("Getting Market Data - COMPLETE");
            return true;
        }
    }
}
