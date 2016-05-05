using System;

namespace TestAICore.DataObjects
{
    public enum TransactionType
    {
        Buy = 837,
        Sell = 5311
    }
    public class Transaction
    {
        public TransactionType Type { get; set; }

        public decimal MarketValue { get; set; }

        public decimal Cost { get; set; }

        public decimal Price { get; set; }

        public string Ticker { get; set; }

        public int Shares { get; set; }

        public DateTime TransactionDate { get; set; }

        public int Id { get; set; }

        public decimal Commission { get; set; }

    }
}
