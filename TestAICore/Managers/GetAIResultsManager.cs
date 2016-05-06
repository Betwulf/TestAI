using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using TestAICore.Repositories;
using TestAICore.DataObjects;

namespace TestAICore.Managers
{
    public class GetAIResultsManager
    {

        private IDocSource mDocDB;
        private GetCalculationsManager mCalcMan = null;
        private string mAzureURL = null;
        private string mApiKey = null;

        public GetAIResultsManager(IDocSource aDocDB)
        {
            mDocDB = aDocDB;
            mCalcMan = new GetCalculationsManager(mDocDB);
            mAzureURL = ConfigurationManager.AppSettings["azureml-endpoint-post"];
            mApiKey = ConfigurationManager.AppSettings["azureml-apikey"];
        }

        public async Task<bool> UpdateMLData(Action<string> updateMessage, DateTime theDate)
        {
            updateMessage("Begin Update Signal: " + theDate);
            var data = mDocDB.CollectionAIData.GetById(theDate.DocIdFromDate());
            CalcResultsSignal(data);
            await mDocDB.CollectionAIData.UpdateAsync(theDate.DocIdFromDate(), data);
            return true;
        }


        public DateTime GetLastAIDataDate()
        {
            var list = mDocDB.CollectionAIData.GetAll();

            var x = list.Max( y => y.AIDate );
            return x;
            
        }


        public async Task<bool> CallAI(string aUniverseName, Action<string> updateMessage, DateTime theDate)
        {
            updateMessage("Begin AI: " + theDate);
            AIDataSeries resultsSeries = new AIDataSeries(theDate);
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                try
                {
                    var data = await mCalcMan.GetDaysCalculations(aUniverseName, updateMessage, x => x.Ticker.StartsWith(letter.ToString()), theDate);
                    var newSeries = new CalcDataSeries();
                    newSeries.CalcDataList = data.CalcDataList;
                    if (data.CalcDataList.Count == 0)
                    {
                        updateMessage("Got 0 count");
                        continue;
                    }
                    Object obj = newSeries.GetWebObject();
                    using (var client = new HttpClient())
                    {

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", mApiKey);
                        client.BaseAddress = new Uri(mAzureURL);
                        HttpResponseMessage response = await client.PostAsJsonAsync("", obj);
                        if (response.IsSuccessStatusCode)
                        {
                            string result = await response.Content.ReadAsStringAsync();
                            JObject dataObject = JObject.Parse(result);
                            JArray jsonArray = (JArray)dataObject["Results"]["output1"]["value"]["Values"];
                            string smallJson = jsonArray.ToString();
                            updateMessage("Result: " + smallJson);

                            foreach (var resultRow in jsonArray)
                            {
                                var newData = new AIData();
                                newData.Ticker = (string)resultRow[0];
                                newData.Date = DateTime.Parse((string)resultRow[1]);
                                newData.BuyScore = (double)resultRow[2];
                                newData.BuyStdDev = (double)resultRow[3];
                                newData.SellScore = (double)resultRow[4];
                                newData.SellStdDev = (double)resultRow[5];
                                resultsSeries.AIDataList.Add(newData);
                            }
                        }
                        else
                        {
                            updateMessage(string.Format("The request failed with status code: {0}", response.StatusCode));

                            // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                            updateMessage(response.Headers.ToString());

                            string responseContent = await response.Content.ReadAsStringAsync();
                            updateMessage(responseContent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    updateMessage(ex.Message);
                }

            }

            resultsSeries = CalcResultsSignal(resultsSeries);
            await mDocDB.CollectionAIData.CreateAsync(resultsSeries);
            updateMessage("Fetching AI Results Completed");
            return true;
        }



        AIDataSeries CalcResultsSignal(AIDataSeries series)
        {
            foreach (var item in series.AIDataList)
            {
                item.Signal = false;
                item.Tier = 0;
                if (item.BuyScore >= 0.9 && item.BuyStdDev <= 0.20)
                {
                    item.Signal = true;
                    item.Tier = 1;
                }
                else if (item.BuyScore >= 0.9 && item.BuyStdDev <= 0.40)
                {
                    item.Signal = true;
                    item.Tier = 2;
                }
                else if (item.BuyScore >= 0.75 && item.BuyStdDev <= 0.40)
                {
                    item.Signal = true;
                    item.Tier = 3;
                }
                else if (item.BuyScore >= 0.6)
                {
                    item.Signal = true;
                    item.Tier = 4;
                }


                if (item.SellScore >= 0.8)
                {
                    item.Signal = false;
                    item.Tier = 7;
                }

            }
            return series;
        }
    }
}
