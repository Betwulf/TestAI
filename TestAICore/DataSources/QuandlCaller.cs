using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestAICore.DataObjects;

namespace TestAICore.DataSources
{
    public class QuandlCaller : IDataCaller
    {
        private string GetURLDate(DateTime date)
        {
            string aMonth = date.Month.ToString().PadLeft(2, '0');
            string aDay = date.Day.ToString().PadLeft(2, '0');
            return date.Year + "-" + aMonth + "-" + aDay;
        }


        private void QuandlWait(Action<string> updateMessage)
        {
            // Wait so Yahoo/DocumentDB doesn't get mad
            var rnd = new Random();
            var delayTime = rnd.Next(3000, 5000);
            updateMessage("Task.Delay for: " + delayTime);
            Task.Delay(delayTime);
        }

        public List<MarketData> GetHistoricalData(string aTicker, DateTime startdate, DateTime enddate, Action<string> updateMessage)
        {
            string startDateString = GetURLDate(startdate);
            string endDateString = GetURLDate(enddate);

            string quandlApiKey = ConfigurationManager.AppSettings["quandl-apikey"];
            StringBuilder theWebAddress = new StringBuilder();
            theWebAddress.Append("https://www.quandl.com/api/v3/datasets/WIKI/");
            theWebAddress.Append(aTicker + ".json?start_date=" + startDateString + "&end_date=" + endDateString + "&api_key=" + quandlApiKey);

            QuandlWait(updateMessage);
            using (var client = new WebClient())
            {
                var json = client.DownloadString(theWebAddress.ToString());
                JObject dataObject = JObject.Parse(json);
                var anyErrors = dataObject.Children().Any(x => x.Path.Contains("errors"));
                if (anyErrors == true)
                {
                    string warning = (string)dataObject["errors"]["quandl_error"]["message"];
                    throw new Exception("Quandl Warning: " + warning);
                }
                else
                {
                    JArray jsonArray = (JArray)dataObject["dataset"]["data"];
                    List<MarketData> newList = new List<MarketData>();
                    foreach (var item in jsonArray)
                    {
                        // Well fuck, going to have to break it up
                        MarketData newData = new MarketData();
                        var itemarray = item.ToArray();
                        newData.PriceDate = DateTime.Parse(itemarray[0].ToString());
                        newData.Open = (decimal)itemarray[1];
                        newData.High = (decimal)itemarray[2];
                        newData.Low = (decimal)itemarray[3];
                        newData.Close = (decimal)itemarray[4];
                        newData.Volume = (int)itemarray[5];
                        newData.AdjClose = (decimal)itemarray[11];
                        newData.Ticker = aTicker;
                        newData.Source = "quandl";
                        newList.Add(newData);
                    }

                    return newList;
                }
            }
        }
    }
}
