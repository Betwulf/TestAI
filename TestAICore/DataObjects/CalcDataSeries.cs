using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }


    public class CalcDataSeries : Document
    {
    
        public CalcDataSeries()
        {
            CalcDataList = new List<CalcData>();
        }
        public void SetFromMarketDataSeries(MarketDataSeries mds)
        {
            Id = mds.Ticker;
            Ticker = mds.Ticker;
            Description = mds.Description;
            Sector = mds.Sector;
        }

        public Object GetWebObject()
        {
            int cols = 17;
            var vals = new string[CalcDataList.Count, cols];
            for (int i = 0; i < CalcDataList.Count; i++)
            {
                var ary = CalcDataList[i].ToStringArray();
                for (int j = 0; j < cols; j++)
                {
                    vals[i, j] = ary[j];
                }
            }
            var scoreRequest = new
            {

                Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"Ticker", "Date", "DailyReturn", "ReturnOpen", "ReturnHigh", "ReturnLow", "NineDayReturn", "FifteenDayReturn", "ThirtyDayReturn", "SixtyDayReturn", "YearsHighPercent", "YearsLowPercent", "MovingAvg9Percent", "MovingAvg15Percent", "MovingAvg30Percent", "MovingAvg60Percent", "StandardDeviation"},
                                Values = vals
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };
            return scoreRequest;
        }

        [JsonProperty(PropertyName = "calcDataList")]
        public List<CalcData> CalcDataList { get; set; }

        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "sector")]
        public string Sector { get; set; }

    }
}
