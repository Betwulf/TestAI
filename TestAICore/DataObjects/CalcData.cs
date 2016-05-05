using System;
using Newtonsoft.Json;


namespace TestAICore.DataObjects
{
    public class CalcData : IComparable<CalcData>
    {
        public void SetFromMarketData(MarketData mds)
        {
            Ticker = mds.Ticker;
            Date = mds.PriceDate;
        }

        public int CompareTo(CalcData other)
        {
            if (other == null) return 1;
            return Date.CompareTo(other.Date);
        }


        public string[] ToStringArray()
        {
            var ary = new string[]
            {
                Ticker.ToString(),
                Date.ToString(),
                DailyReturn.ToString(),
                ReturnOpen.ToString(),
                ReturnHigh.ToString(),
                ReturnLow.ToString(),
                NineDayReturn.ToString(),
                FifteenDayReturn.ToString(),
                ThirtyDayReturn.ToString(),
                SixtyDayReturn.ToString(),
                YearsHighPercent.ToString(),
                YearsLowPercent.ToString(),
                MovingAvg9Percent.ToString(),
                MovingAvg15Percent.ToString(),
                MovingAvg30Percent.ToString(),
                MovingAvg60Percent.ToString(),
                StandardDeviation.ToString()
            };
            return ary;
        }

        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "dailyReturn")]
        public decimal DailyReturn { get; set; }

        [JsonProperty(PropertyName = "returnOpen")]
        public decimal ReturnOpen { get; set; }

        [JsonProperty(PropertyName = "returnHigh")]
        public decimal ReturnHigh { get; set; }

        [JsonProperty(PropertyName = "returnLow")]
        public decimal ReturnLow { get; set; }

        [JsonProperty(PropertyName = "nineDayReturn")]
        public decimal NineDayReturn { get; set; }

        [JsonProperty(PropertyName = "fifteenDayReturn")]
        public decimal FifteenDayReturn { get; set; }

        [JsonProperty(PropertyName = "thirtyDayReturn")]
        public decimal ThirtyDayReturn { get; set; }

        [JsonProperty(PropertyName = "sixtyDayReturn")]
        public decimal SixtyDayReturn { get; set; }

        [JsonProperty(PropertyName = "yearsHighPercent")]
        public decimal YearsHighPercent { get; set; }

        [JsonProperty(PropertyName = "yearsLowPercent")]
        public decimal YearsLowPercent { get; set; }

        [JsonProperty(PropertyName = "movingAvg9Percent")]
        public decimal MovingAvg9Percent { get; set; }

        [JsonProperty(PropertyName = "movingAvg15Percent")]
        public decimal MovingAvg15Percent { get; set; }

        [JsonProperty(PropertyName = "movingAvg30Percent")]
        public decimal MovingAvg30Percent { get; set; }

        [JsonProperty(PropertyName = "movingAvg60Percent")]
        public decimal MovingAvg60Percent { get; set; }

        [JsonProperty(PropertyName = "standardDeviation")]
        public double StandardDeviation { get; set; }

        
    }
}
