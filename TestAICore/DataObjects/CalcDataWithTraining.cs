using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class CalcDataWithTraining : CalcData
    {
        public void SetFromCalcData(CalcData cd)
        {
            Date = cd.Date;
            Ticker = cd.Ticker;
            DailyReturn = cd.DailyReturn;
            ReturnOpen = cd.ReturnOpen;
            ReturnHigh = cd.ReturnHigh;
            ReturnLow = cd.ReturnLow;
            NineDayReturn = cd.NineDayReturn;
            FifteenDayReturn = cd.FifteenDayReturn;
            ThirtyDayReturn = cd.ThirtyDayReturn;
            SixtyDayReturn = cd.SixtyDayReturn;
            YearsHighPercent = cd.YearsHighPercent;
            YearsLowPercent = cd.YearsLowPercent;
            MovingAvg9Percent = cd.MovingAvg9Percent;
            MovingAvg15Percent = cd.MovingAvg15Percent;
            MovingAvg30Percent = cd.MovingAvg30Percent;
            MovingAvg60Percent = cd.MovingAvg60Percent;
            StandardDeviation = cd.StandardDeviation;
        }

        [JsonProperty(PropertyName = "buySignal")]
        public int BuySignal { get; set; }

        [JsonProperty(PropertyName = "sellSignal")]
        public int SellSignal { get; set; }

    }
}
