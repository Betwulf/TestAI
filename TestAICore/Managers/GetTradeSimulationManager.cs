using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestAICore.Repositories;
using TestAICore.DataObjects;

namespace TestAICore.Managers
{
    public class GetTradeSimulationManager
    {

        private IDocDB mDocDB;

        private readonly string mCashTicker;

        private int transactionID;
        // generate unique IDs for transactions
        public int NextTransactionID
        {
            get { return transactionID++; }
            set { transactionID = value; }
        }

        private readonly string mBenchmarkTicker;

        public GetTradeSimulationManager(IDocDB aDocDB)
        {
            NextTransactionID = 1;
            mDocDB = aDocDB;
            mCashTicker = "$";
            mBenchmarkTicker = ConfigurationManager.AppSettings["returns-benchmark-ticker"]; 
        }



        private decimal runningCash { get; set; }

        public TradeSimulation sim { get; set; }

        private List<Position> currPositions { get; set; }

        private List<Transaction> buyTransactions { get; set; }

        private decimal commissionPerShare { get; set; }

        private decimal totalCommission { get; set; }

        private List<MarketData> benchmarkMarketData { get; set; }



        public async Task<TradeSimulation> SimulateBalancedPortfolio(Action<string> updateMessage, DateTime aStartDate, DateTime anEndDate)
        {
            // Parameters 
            runningCash = 10000000M;
            decimal maxOwnershipPercent = 0.05M;
            decimal thresholdForRebalancePercent = 0.11M;
            decimal minimumBuyPercent = 0.003M;
            commissionPerShare = 0.005M;
            totalCommission = 0M;
            benchmarkMarketData = GetMarketData(mBenchmarkTicker);


            updateMessage("Starting Trade Sim - Simulate Balanced Portfolio");
            sim = new TradeSimulation() { StartDate = aStartDate, EndDate = anEndDate, RunDate = DateTime.Now };
            currPositions = new List<Position>();
            buyTransactions = new List<Transaction>();
            DateTime currDate = new DateTime(sim.StartDate.Ticks);
            DateTime yesterDate = currDate.AddDays(-1);
            sim.Parameters.Add("SimulationType", "SimulatedBalancedPortfolio");
            sim.Parameters.Add("runningCash", runningCash.ToString());
            sim.Parameters.Add("maxOwnershipPercent", maxOwnershipPercent.ToString());
            sim.Parameters.Add("thresholdForRebalancePercent", thresholdForRebalancePercent.ToString());
            sim.Parameters.Add("minimumBuyPercent", minimumBuyPercent.ToString());
            sim.Parameters.Add("commissionPerShare", commissionPerShare.ToString());
            while (sim.EndDate >= currDate)
            {
                updateMessage("Sim day: " + currDate.DocIdFromDate());

                // refresh yesterday's positions
                currPositions = UpdatePositions(currPositions, currDate, updateMessage);
                CalcDailyReturns(yesterDate); // For yesterday

                // Get today's buys and sells
                var buyPredictions = new List<AIData>();
                var sellPredictions = new List<AIData>();
                // Get today's predictions
                var dailyPredictions = mDocDB.CollectionAIData.GetById(currDate.AddDays(-1).DocIdFromDate());
                if (dailyPredictions == null) { throw new Exception("No AI data for: " + currDate.AddDays(-1).DocIdFromDate()); }
                if (dailyPredictions.AIDataList.Count != 0) // if it isn't a holiday
                {
                    foreach (var prediction in dailyPredictions.AIDataList)
                    {
                        // Check for a buy / sell signal, add to the list
                        if (prediction.Signal)
                        { buyPredictions.Add(prediction); }
                        else
                        { sellPredictions.Add(prediction); }
                    }

                    // TEMP: List out today's predictions
                    if (currDate == DateTime.Today)
                    {
                        foreach (var item in buyPredictions)
                        {
                            updateMessage($"LATEST BUY PREDICTIONS: {item.Ticker}");
                        }
                    }

                    // sell
                    foreach (var prediction in sellPredictions)
                    {
                        var pos = currPositions.Find(x => x.Ticker == prediction.Ticker);
                        if (pos != null)
                        {
                            var sellTrx = SellPosition(pos, currDate, updateMessage, "Sell Signal");
                        }
                    }
                    // Calculate even distributed value of each holding
                    int countTodayPositions = buyPredictions.Count > 0 ? buyPredictions.Count : 1;
                    decimal totalValue = currPositions.Sum(x => x.MarketValue) + runningCash;
                    updateMessage("                   Total MV: " + String.Format("{0:C}", totalValue));
                    decimal maxOwnershipMktValue = maxOwnershipPercent * totalValue;
                    decimal estimatedEvenMktValue = (totalValue / countTodayPositions);
                    estimatedEvenMktValue = estimatedEvenMktValue > maxOwnershipMktValue ? maxOwnershipMktValue : estimatedEvenMktValue;

                    // now figure out how much our existing positions may have to change
                    bool rebalance = false;
                    if (currPositions.Count != 0)
                    { rebalance = Math.Abs((currPositions.First().MarketValue - estimatedEvenMktValue) / estimatedEvenMktValue) > thresholdForRebalancePercent; }
                    if (rebalance)
                    {
                        updateMessage("Rebalancing...");
                        // then we rebalance - by selling what we have and buying again, making separate bets to keep it simple
                        var tempList = new List<Position>(currPositions);
                        foreach (var pos in tempList)
                        {
                            var adjustedClose = pos.MarketValue / pos.Shares;
                            var prediction = buyPredictions.Find(x => x.Ticker == pos.Ticker);
                            if (prediction == null) { updateMessage("No buyPredictions for: " + pos.Ticker); }
                            else
                            {
                                var sellTrx = SellPosition(pos, currDate, updateMessage, "Rebalance");
                                // TODO: Can't change iterator currPositions 
                                var buyTrx = BuyPosition(prediction, estimatedEvenMktValue, currDate, updateMessage);
                                buyPredictions.Remove(prediction);
                            }
                        }
                        foreach (var prediction in buyPredictions)
                        {
                            var buyTrx = BuyPosition(prediction, estimatedEvenMktValue, currDate, updateMessage);
                        }
                    }
                    else
                    {
                        // we keep our existing positions, do we have cash to buy?
                        //buyPredictions.Sort(new AIDataScoreComparer());
                        if (estimatedEvenMktValue / totalValue > minimumBuyPercent)
                        {
                            Console.WriteLine("Cash to spare on more trades: " + runningCash.ToString());
                            // we can buy all
                            foreach (var prediction in buyPredictions)
                            {
                                // If we don't already have a position
                                if (currPositions.Find(x => x.Ticker == prediction.Ticker) == null)
                                {
                                    var buyTrx = BuyPosition(prediction, estimatedEvenMktValue, currDate, updateMessage);
                                }
                            }

                        }

                    }
                }

                // Add cash as a position
                var todayCash = new Position() { Date = currDate, MarketValue = runningCash, Shares = 1, Ticker = mCashTicker, TransactionId = -1 };
                sim.Positions.Add(todayCash);

                // Roll Dates
                yesterDate = currDate;
                currDate = currDate.NextBusinessDay();
            }

            // Close out open bets
            currPositions = UpdatePositions(currPositions, currDate, updateMessage);
            foreach (var pos in currPositions)
            {
                var sellTrx = SellPosition(pos, currDate, updateMessage, "End of Simulation", false);
            }
            var lastDayCash = new Position() { Date = currDate, MarketValue = runningCash, Shares = 1, Ticker = mCashTicker, TransactionId = -1 };
            sim.Positions.Add(lastDayCash);
            CalcDailyReturns(currDate); // Final Returns

            // Save Commission paid
            sim.TotalCommissionPaid = totalCommission;


            updateMessage("PROFIT AND LOSS: " + String.Format("{0:C}", sim.NetProfitLoss));
            updateMessage("Commission Paid: " + String.Format("{0:C}", sim.TotalCommissionPaid));
            await SaveSim(updateMessage);
            return sim;
        }


        private void CalcDailyReturns(DateTime currDate)
        {
            var yesterdayDate = currDate.AddDays(-1);
            var yesterdayMV = (from pos in sim.Positions where pos.Date == yesterdayDate select pos.MarketValue).Sum();
            var todayMV = (from pos in sim.Positions where pos.Date == currDate select pos.MarketValue).Sum();
            var benchmarkTodayMV = (from md in benchmarkMarketData where md.PriceDate == currDate && md.Ticker == mBenchmarkTicker select md.AdjClose).DefaultIfEmpty(0).First();
            var benchmarkYesterdayMV = (from md in benchmarkMarketData where md.PriceDate < currDate && md.Ticker == mBenchmarkTicker orderby md.PriceDate select md.AdjClose).LastOrDefault();
            var returnVal = yesterdayMV == 0 ? 1 : (todayMV / yesterdayMV);
            var benchmarkReturnVal = benchmarkTodayMV == 0 ? 1 : (benchmarkTodayMV / benchmarkYesterdayMV);

            sim.Returns.Add( new DailyReturn() { Date = currDate, DayReturn = returnVal, BenchmarkReturn = benchmarkReturnVal } );
        }

        private Bet MakeBet(Transaction buy, Transaction sell, Position oldPos, string aReason)
        {
            if (buy == null)
            {
                Console.WriteLine("MakeBet - Null?");
            }
            var aBet = new Bet() { BuyDate = buy.TransactionDate, SellDate = sell.TransactionDate,
                Ticker = buy.Ticker, ScoredLabelMean = oldPos.ScoredLabelMean,
                ScoredLabelStandardDeviation = oldPos.ScoredLabelStandardDeviation, Tier = oldPos.Tier, Reason = aReason };
            aBet.NotionalValue = -buy.MarketValue; // Buy trx MktVal is negative
            aBet.CommissionPaid = buy.Commission + sell.Commission;
            aBet.ProfitLoss = sell.MarketValue + buy.MarketValue;
            aBet.NetProfitLoss = aBet.ProfitLoss - aBet.CommissionPaid;
            aBet.Return = sell.MarketValue / -buy.MarketValue;
            aBet.NetReturn = sell.MarketValue / (-buy.MarketValue + aBet.CommissionPaid);
            aBet.DurationInDays = (sell.TransactionDate - buy.TransactionDate).Days;
            sim.Bets.Add(aBet);
            sim.NetProfitLoss += sell.MarketValue + buy.MarketValue - aBet.CommissionPaid;
            return aBet;
        }




        private Transaction SellPosition(Position oldPosition, DateTime sellDate, Action<string> updateMessage, string aReason, bool removePosition = true)
        {
            MarketData todayMktData = GetMarketData(sellDate, oldPosition.Ticker);
            if (todayMktData == null) return null; // No market data today (maybe a holiday?), try trading tomorrow

            // Create Sell Transaction
            decimal tradeCommission = oldPosition.Shares * commissionPerShare;
            totalCommission += tradeCommission;
            decimal realMktValue = oldPosition.Shares * todayMktData.AdjClose;

            // Calc cash
            var cashOut = realMktValue - tradeCommission;
            runningCash += cashOut;

            var sellTrx = new Transaction()
            { Type = TransactionType.Sell, Price = todayMktData.AdjClose, Ticker = oldPosition.Ticker, Cost = cashOut,
                MarketValue = realMktValue, Shares = oldPosition.Shares, TransactionDate = sellDate, Id = NextTransactionID, Commission = tradeCommission };
            sim.Transactions.Add(sellTrx);


            // Make Bet
            var buyTrx = buyTransactions.Find(x => x.Id == oldPosition.TransactionId);
            MakeBet(buyTrx, sellTrx, oldPosition, aReason);
            buyTransactions.Remove(buyTrx);
            sim.Transactions.Add(buyTrx);
            if (removePosition) currPositions.Remove(oldPosition);
            return sellTrx;
        }




        private Transaction BuyPosition(AIData aiData, decimal roughDollarAmount, DateTime currDate, Action<string> updateMessage)
        {
            MarketData todayMktData = GetMarketData(currDate, aiData.Ticker);
            if (todayMktData == null) return null; // No market data today (maybe a holiday?), try trading tomorrow

            // Create BUY transaction
            int shares = (int)(roughDollarAmount / todayMktData.AdjClose);
            decimal realMktValue = -(shares * todayMktData.AdjClose);

            decimal tradeCommission = shares * commissionPerShare;
            totalCommission += tradeCommission;

            // Calc cash
            var cashOut = realMktValue - tradeCommission;
            runningCash += cashOut;

            var buyTrx = new Transaction()
            { Type = TransactionType.Buy, Price = todayMktData.AdjClose, Ticker = aiData.Ticker, MarketValue = realMktValue, Cost = cashOut,
                Shares = shares, TransactionDate = currDate, Id = NextTransactionID, Commission= tradeCommission };
            buyTransactions.Add(buyTrx);


            // Create Position
            var pos = new Position() { Date = currDate, Shares = shares, Ticker = aiData.Ticker, TransactionId = buyTrx.Id, MarketValue = -realMktValue,
                Tier = aiData.Tier, ScoredLabelMean = aiData.BuyScore, ScoredLabelStandardDeviation = aiData.BuyStdDev };
            currPositions.Add(pos);
            return buyTrx;
        }





        private List<Position> UpdatePositions(List<Position> yesterdayPositions, DateTime aCurrDate, Action<string> updateMessage)
        {
            var newPositions = new List<Position>();
            var currDate = aCurrDate;
            foreach (var oldPos in yesterdayPositions)
            {
                sim.Positions.Add(oldPos); // Add yesterday's positions to sim
                var marketDataSeries = mDocDB.CollectionMarketData.GetById(oldPos.Ticker);
                if (marketDataSeries == null) { throw new Exception("No marketData for: " + oldPos.Ticker); }
                // Assume dates are in order
                //marketDataSeries.MarketDataList.Sort(); // Make sure Dates are in order
                var lastDate = marketDataSeries.MarketDataList.Last().PriceDate;
                // Look for the data, make sure we haven't past the last day of data.
                MarketData todayMktData = marketDataSeries.MarketDataList.Find(x => x.PriceDate == currDate);
                if (todayMktData == null)
                {
                    if (currDate > lastDate)
                    {
                        // then close position and create sell transaction for yesterday
                        SellPosition(oldPos, lastDate, updateMessage, "End of Data", false);
                        continue;
                    }
                }
                if (todayMktData != null)
                {
                    // found new data, create this day's positions
                    newPositions.Add(new Position() { Date = currDate, Shares = oldPos.Shares, Ticker = oldPos.Ticker, TransactionId = oldPos.TransactionId, MarketValue = todayMktData.AdjClose * oldPos.Shares, Tier = oldPos.Tier, ScoredLabelMean = oldPos.ScoredLabelMean, ScoredLabelStandardDeviation = oldPos.ScoredLabelStandardDeviation });
                }
                else
                {
                    // Must be a holiday roll existing positions forward
                    newPositions.Add(new Position() { Date = currDate, Shares = oldPos.Shares, Ticker = oldPos.Ticker, TransactionId = oldPos.TransactionId, MarketValue = oldPos.MarketValue, Tier = oldPos.Tier, ScoredLabelMean = oldPos.ScoredLabelMean, ScoredLabelStandardDeviation = oldPos.ScoredLabelStandardDeviation });
                }
            }
            return newPositions;
        }





        private async Task<bool> SaveSim(Action<string> updateMessage)
        {

            updateMessage("Saving...");

            var compressedBets = CompressBets();

            var dir = ((DocCollectionFile<TradeSimulation>)mDocDB.CollectionTradeSimulation).GetDir;
            var filename = dir + "\\" + sim.Id;
            sim.Transactions.CreateCSVFromGenericList(filename + " TradeSim.Transactions.csv");
            sim.Bets.CreateCSVFromGenericList(filename + " TradeSim.Bets.csv");
            compressedBets.CreateCSVFromGenericList(filename + " TradeSim.CompressedBets.csv");
            sim.Positions.CreateCSVFromGenericList(filename + " TradeSim.Positions.csv");
            sim.Returns.CreateCSVFromGenericList(filename + " TradeSim.Returns.csv");

            sim.Transactions.Clear(); // too big otherwise, use CSV
            sim.Positions.Clear(); // too big otherwise, use CSV
            sim.Bets.Clear(); // too big otherwise, use CSV
            await mDocDB.CollectionTradeSimulation.CreateAsync(sim);
            updateMessage("Trade Sim Complete");
            return true;
        }

        


        private List<Bet> CompressBets()
        {
            //Review Bets
            var newBets = new List<Bet>();
            var tickers = (from bet in sim.Bets select bet.Ticker).Distinct();
            foreach (var ticker in tickers)
            {
                var tickerBets = from bet in sim.Bets where bet.Ticker == ticker select bet;
                var listBets = tickerBets.ToList<Bet>();
                listBets.Sort(); // Date sorted
                if (listBets.Count >= 2)
                {
                    bool rebalancing = true;
                    for (int i = 0; i < listBets.Count; i++)
                    {
                        var newBet = new Bet() { Ticker = ticker, Return = 1M, NetReturn = 1M, CommissionPaid = 0M, NetProfitLoss = 0M, NotionalValue = 0M, ProfitLoss = 0M };
                        rebalancing = true;
                        var startIndex = i;

                        while (rebalancing)
                        {
                            if (i >= listBets.Count) { rebalancing = false; i--; }
                            else if (listBets[i].Reason != "Rebalance")
                            {
                                rebalancing = false;
                                newBet.Return *= listBets[i].Return;
                                newBet.NetReturn *= listBets[i].NetReturn;
                                newBet.ProfitLoss += listBets[i].ProfitLoss;
                                newBet.NetProfitLoss += listBets[i].NetProfitLoss;
                                newBet.CommissionPaid += listBets[i].CommissionPaid;
                            }
                            else
                            {
                                newBet.Return *= listBets[i].Return;
                                newBet.NetReturn *= listBets[i].NetReturn;
                                newBet.ProfitLoss += listBets[i].ProfitLoss;
                                newBet.NetProfitLoss += listBets[i].NetProfitLoss;
                                newBet.CommissionPaid += listBets[i].CommissionPaid;
                                i++;
                            }
                        }

                        if (startIndex == i)
                        {
                            // then no relanacing, this bet exists as is
                            newBets.Add(listBets[i]);
                        }
                        else
                        {
                            newBet.NotionalValue = listBets[startIndex].NotionalValue;
                            newBet.BuyDate = listBets[startIndex].BuyDate;
                            newBet.SellDate = listBets[i].SellDate;
                            newBet.DurationInDays = (newBet.SellDate - newBet.BuyDate).Days;
                            newBet.Reason = listBets[i].Reason;
                            newBet.ScoredLabelMean = listBets[startIndex].ScoredLabelMean;
                            newBet.ScoredLabelStandardDeviation = listBets[startIndex].ScoredLabelStandardDeviation;
                            newBet.Tier = listBets[startIndex].Tier;
                            newBets.Add(newBet);
                        }

                    }
                }
                else if (listBets.Count == 1)
                {
                     
                    newBets.Add(listBets[0]);
                }
                else
                {
                    Console.WriteLine($"Compress Bets: No trades for {ticker}");
                }
            }
            return newBets;
        }



        private MarketData GetMarketData(DateTime aDate, string aTicker)
        {
            var marketDataSeries = mDocDB.CollectionMarketData.GetById(aTicker);
            if (marketDataSeries == null) { throw new Exception("No marketData for: " + aTicker); }
            return marketDataSeries.MarketDataList.Find(x => x.PriceDate == aDate);
        }



        private List<MarketData> GetMarketData(string aTicker)
        {
            var marketDataSeries = mDocDB.CollectionMarketData.GetById(aTicker);
            if (marketDataSeries == null) { throw new Exception("No marketData for: " + aTicker); }
            return marketDataSeries.MarketDataList;
        }

    }
}
