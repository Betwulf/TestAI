using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TestAICore.DataObjects;
using TestAICore.Repositories;
using TestAICore.Managers;
using TestAICore;


namespace TestAI
{
    public class AIController
    {
        private IDocSource mDocDB;

        public AIController()
        {
            mDocDB = new DocFile();
        }



        public async Task<bool> CreateUniverse(string aUniverse, Action<string> updateMessage)
        {
            try
            {
                var um = new GetUniverseManager(mDocDB);
                await um.CreateUniverse(updateMessage);
            }
            catch (Exception gex)
            {
                updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                return false;
            }
            return true;
        }



        public async Task<bool> UpdateMarketData(string aUniverse, Action<string> updateMessage)
        {
            Universe u = mDocDB.CollectionUniverse.GetById(aUniverse);
            if (u == null) updateMessage("Can't find Universe");

            return await Task.Run(async () =>
            {
                try
                {
                    var mgr = new GetMarketDataManager(mDocDB);
                    await mgr.GetHistoryFromUniverse(aUniverse, updateMessage);
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return false;
                }
                return true;
            });
        }



        public async Task<bool> GetMoreHistoricalMarketData(string aUniverse, Action<string> updateMessage)
        {
            Universe u = mDocDB.CollectionUniverse.GetById(aUniverse);
            if (u == null) updateMessage("Can't find Universe");

            return await Task.Run(async () =>
            {
                try
                {
                    var mgr = new GetMarketDataManager(mDocDB);
                    await mgr.GetHistoryFromUniverse(aUniverse, updateMessage, true).
                        ContinueWith(t => updateMessage(t.Exception.ToString()),
                        TaskContinuationOptions.NotOnRanToCompletion);
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return false;
                }
                return true;
            });
        }



        public async Task<bool> DeleteAllData(Action<string> updateMessage)
        {
            return await mDocDB.DeleteDatabase(updateMessage);
        }



        public async Task<DateTime?> UpdateCalcData(string aUniverseName, Action<string> updateMessage)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mgr = new GetCalculationsManager(mDocDB);
                    return mgr.CreateCalculations(aUniverseName, updateMessage);
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return null;
                }
            });
        }



        public async Task<bool> CalculateTrainingData(string aUniverseName, Action<string> updateMessage)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var mgr = new GetCalculationsManager(mDocDB);
                    return await mgr.CalculateTrainingSignal(aUniverseName, updateMessage);
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return false;
                }
            });
        }



        public async Task<bool> ClearCalcData(Action<string> updateMessage)
        {
            // TODO: Not good call
            return await mDocDB.DeleteCalcDataWithTrainingCollectionAsync(updateMessage);

        }



        public async Task<bool> UpdateMLData(string aUniverseName, DateTime startDate, DateTime endDate, Action<string> updateMessage)
        {
            updateMessage($" - Calling UpdateMLData for date range: {startDate.DocIdFromDate()} to {endDate.DocIdFromDate()}");
            return await Task.Run(async () =>
            {
                var mgr = new GetAIResultsManager(mDocDB);
                try
                {
                    int days = (endDate - startDate).Days;
                    for (int i = 0; i < days; i++)
                    {
                        DateTime thisDay = startDate.AddDays(i);
                        await mgr.CallAI(aUniverseName, updateMessage, thisDay);
                    }
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return false;
                }
                return true;
            });
        }



        public async Task<bool> RefreshMLData(DateTime startDate, DateTime endDate, Action<string> updateMessage)
        {
            var mgr = new GetAIResultsManager(mDocDB);

            try
            {
                int days = (endDate - startDate).Days;
                for (int i = 0; i <= days; i++)
                {
                    DateTime thisDay = startDate.AddDays(i);
                    // TODO: Have to await this, Assuming can't pole the AI web service too much, it will choke
                    // but should probably test this
                    await mgr.UpdateMLData(updateMessage, thisDay);
                }
                return true;
            }
            catch (Exception gex)
            {
                updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                return false;
            }
        }





        public async Task<bool> RunSimulation(DateTime startDate, DateTime endDate, Action<string> updateMessage)
        {
            updateMessage($" - Calling RunSimulation for date range: {startDate.DocIdFromDate()} to {endDate.DocIdFromDate()}");
            return await Task.Run(() =>
            {
                try
                {
                    var mgr = new GetTradeSimulationManager(mDocDB);
                    mgr.SimulateBalancedPortfolio(updateMessage, startDate, endDate).
                        ContinueWith(t => updateMessage(t.Exception.ToString()),
                        TaskContinuationOptions.NotOnRanToCompletion);
                }
                catch (Exception gex)
                {
                    updateMessage("Error: " + JsonConvert.SerializeObject(gex));
                    return false;
                }
                return true;
            });
        }




        public async Task<bool> DailyFullUpdate(string aUniverseName, DateTime startDate, DateTime endDate, Action<string> updateMessage)
        {
            // FRESH MARKET DATA
            bool success = await UpdateMarketData(aUniverseName, updateMessage);
            DateTime? latestDataDate = null;

            // CALC DATA
            if (success) { latestDataDate = await UpdateCalcData(aUniverseName, updateMessage); } else { updateMessage("Received false return from UpdateMarketData"); return false; }

            // AI DATA
            if (latestDataDate.HasValue)
            {
                updateMessage($"CALCED Data up to {latestDataDate.Value.DocIdFromDate()}");
                var mgrAI = new GetAIResultsManager(mDocDB);
                DateTime lastAIDate = mgrAI.GetLastAIDataDate().AddDays(1);
                success = await UpdateMLData(aUniverseName, lastAIDate, endDate, updateMessage);
            }
            else { updateMessage("Received no Date from UpdateCalcData"); return false; }

            // SIMULATION
            if (success) { await RunSimulation(startDate, endDate, updateMessage); }
            return true;
        }

    }
}
