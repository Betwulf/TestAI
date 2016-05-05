using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using TestAICore.DataObjects;

namespace TestAICore.DataSources
{
    public class YahooCaller : IDataCaller 
    {

        private void YahooWait(Action<string> updateMessage)
        {
            // Wait so Yahoo/DocumentDB doesn't get mad
            var rnd = new Random();
            var delayTime = rnd.Next(3000, 6000);
            updateMessage("Task.Delay for: " + delayTime);
            Thread.Sleep(delayTime);
        }


        private string GetYahooDate(DateTime date)
        {
            string aMonth = date.Month.ToString().PadLeft(2, '0');
            string aDay = date.Day.ToString().PadLeft(2, '0');
            return date.Year + "-" + aMonth + "-" + aDay;
        }

        public List<MarketData> GetHistoricalData(string aTicker, DateTime startdate, DateTime enddate, Action<string> updateMessage)
        {
            string startDateString = GetYahooDate(startdate);
            string endDateString = GetYahooDate(enddate);


            StringBuilder theWebAddress = new StringBuilder();
            theWebAddress.Append("http://query.yahooapis.com/v1/public/yql?");

            string yquery = "select * from yahoo.finance.historicaldata where symbol = '" + aTicker + "' and startDate = '" + startDateString + "' and endDate = '" + endDateString + "'";
            theWebAddress.Append("q=" + System.Web.HttpUtility.UrlEncode(yquery));
            theWebAddress.Append("&format=json");
            theWebAddress.Append("&diagnostics=true");
            theWebAddress.Append("&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
            theWebAddress.Append("&callback=");

            YahooWait(updateMessage);
            using (var client = new WebClient())
            {
                var json = client.DownloadString(theWebAddress.ToString());
                JObject dataObject = JObject.Parse(json);
                int queryCount = (int)dataObject["query"]["count"];
                if (queryCount == 0)
                {
                    string yahooWarning = (string)dataObject["query"]["diagnostics"]["warning"];
                    throw new Exception("Yahoo Warning: " + yahooWarning);
                }
                else
                {
                    JArray jsonArray = (JArray)dataObject["query"]["results"]["quote"];
                    string smallJson = jsonArray.ToString();

                    List<MarketData> newList = JsonConvert.DeserializeObject<List<MarketData>>(smallJson);

                    foreach (var item in newList)
                    {
                        item.Source = "Yahoo";
                    }
                    return newList;
                }
            }
        }
    }
}
