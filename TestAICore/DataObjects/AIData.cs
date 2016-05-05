using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class AIData
    {
        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "BuyScore")]
        public double BuyScore { get; set; }

        [JsonProperty(PropertyName = "BuyStdDev")]
        public double BuyStdDev { get; set; }

        [JsonProperty(PropertyName = "SellScore")]
        public double SellScore { get; set; }

        [JsonProperty(PropertyName = "SellStdDev")]
        public double SellStdDev { get; set; }

        [JsonProperty(PropertyName = "signal")]
        public bool Signal { get; set; }

        [JsonProperty(PropertyName = "tier")]
        public int Tier { get; set; }

    }





    public class AIDataScoreComparer : IComparer<AIData>
    {
        public int Compare(AIData x, AIData y)
        {
            if (x == null) return 1;
            return x.BuyScore.CompareTo(y.BuyScore);
        }
    }
}
