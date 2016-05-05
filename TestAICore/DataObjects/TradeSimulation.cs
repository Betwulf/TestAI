using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class TradeSimulation : Document
    {

        public TradeSimulation()
        {
            Transactions = new List<Transaction>();
            Positions = new List<Position>();
            Bets = new List<Bet>();
            Parameters = new Dictionary<string, string>();
            Returns = new List<DailyReturn>();
            RunDate = DateTime.Now;
            NetProfitLoss = 0M;
        }

        [JsonProperty(PropertyName = "returns")]
        public List<DailyReturn> Returns { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public List<Transaction> Transactions { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public List<Bet> Bets { get; set; }


        private DateTime runDate;
        [JsonProperty(PropertyName = "runDate")]
        public DateTime RunDate {
            get { return runDate; }
            set { runDate = value;  Id = runDate.DocIdFromDate();  }
        }

        [JsonProperty(PropertyName = "positions")]
        public List<Position> Positions { get; set; }


        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }


        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate { get; set; }


        [JsonProperty(PropertyName = "netProfitLoss")]
        public decimal NetProfitLoss { get; set; }


        [JsonProperty(PropertyName = "parameters")]
        public Dictionary<string, string> Parameters { get; set; }

        [JsonProperty(PropertyName = "totalCommissionPaid")]
        public decimal TotalCommissionPaid { get; set; }


    }
}
