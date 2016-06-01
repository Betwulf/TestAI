using System;

namespace TestAICore.DataObjects
{
    public class Bet : IComparable<Bet>
    {
        public DateTime BuyDate { get; set; }

        public DateTime SellDate { get; set; }

        public string Ticker { get; set; }

        public decimal Price { get; set; }

        public decimal NotionalValue { get; set; }

        public decimal CommissionPaid { get; set; }

        public decimal Return { get; set; }

        public decimal NetReturn { get; set; }

        public decimal ProfitLoss { get; set; }

        public decimal NetProfitLoss { get; set; }

        public decimal ZeroBasedReturn { get { return Return - 1; } }

        public int DurationInDays { get; set; }

        public int Tier { get; set; }

        public double ScoredLabelMean { get; set; }

        public double ScoredLabelStandardDeviation { get; set; }

        public string Reason { get; set; }


        public int CompareTo(Bet other)
        {
            if (other == null) return 1;
            return BuyDate.CompareTo(other.BuyDate);
        }

    }
}
