using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace TestAICore.DataObjects
{
    public class MarketData : IComparable<MarketData>
    {
        [JsonProperty(PropertyName = "Symbol")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime PriceDate { get; set; }

        [JsonProperty(PropertyName = "Adj_Close")]
        public decimal AdjClose { get; set; }

        public decimal Close { get; set; }

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public int Volume { get; set; }

        public int CompareTo(MarketData other)
        {
            if (other == null) return 1;
            return PriceDate.CompareTo(other.PriceDate);
        }

        public string Source { get; set; }

    }

    public class MarketDataReverseComparer : IComparer<MarketData>
    {
        public int Compare(MarketData x, MarketData y)
        {
            if (x == null) return 1;
            return -x.PriceDate.CompareTo(y.PriceDate);
        }
    }
}
