using System;

namespace TestAICore.DataObjects
{
    public class Position
    {
        public DateTime Date { get; set; }

        public int Shares { get; set; }

        public string Ticker { get; set; }

        public decimal MarketValue { get; set; }

        public int TransactionId { get; set; }

        public int Tier { get; set; }

        public double ScoredLabelMean { get; set; }

        public double ScoredLabelStandardDeviation { get; set; }


        public string Id
        {
            get { return Date.DocIdFromDate() + Ticker; }
        }

    }
}
