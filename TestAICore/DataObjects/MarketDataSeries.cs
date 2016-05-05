using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace TestAICore.DataObjects
{
    public class MarketDataSeries : Document
    {
        public void SetFromConstituent(Constituent c)
        {
            Id = c.Ticker;
            Ticker = c.Ticker;
            Description = c.Description;
            Sector = c.Sector;
        }

        [JsonProperty(PropertyName = "marketDataList")]
        public List<MarketData> MarketDataList { get; set; }

        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "sector")]
        public string Sector { get; set; }

        /// <summary>
        /// Current values require adjustment of historical values. 
        /// Stitching needs to calculate these changes to adjusted price.
        /// The passed in List needs to have at least one overlapped value with this
        /// Results in this MarketDataSeries merging data from itself and the parameter.
        /// </summary>
        /// <param name="newSeries">a series of new values for the same security.</param>
        public void StitchTogether(List<MarketData> newSeries, bool isHistoricalSeries = false)
        {
            newSeries.Sort();
            MarketDataList.Sort();
            if (isHistoricalSeries)
            {
                var tempSeries = MarketDataList;
                MarketDataList = newSeries;
                newSeries = tempSeries;
            }

            var lastMarketData = MarketDataList.Last();
            var overlap = newSeries.Find(x => x.PriceDate == lastMarketData.PriceDate);

            if (overlap == null)
            { throw new ArgumentException("StitchTogether requires overlapped reords to correctly calc adjustment"); }
            if (overlap.AdjClose != lastMarketData.AdjClose)
            {
                // then adjustment is needed
                decimal overlapAdjustValue = overlap.Close / overlap.AdjClose;
                foreach (var item in MarketDataList)
                {
                    item.AdjClose = item.AdjClose / overlapAdjustValue;
                }
            }
            // remove overlap(s)
            newSeries.RemoveAll(x => x.PriceDate <= lastMarketData.PriceDate);
            MarketDataList.AddRange(newSeries);
        }
    }
}
