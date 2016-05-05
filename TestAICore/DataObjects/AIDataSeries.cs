using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TestAICore;

namespace TestAICore.DataObjects
{
    public class AIDataSeries : Document
    {
        public AIDataSeries()
        {
            AIDataList = new List<AIData>();
        }
        public AIDataSeries(DateTime anAIDate)
        {
            AIDate = anAIDate;
            AIDataList = new List<AIData>();
        }

        private DateTime aiDate;


        [JsonProperty(PropertyName = "aiDate")]
        public DateTime AIDate
        {
            get { return aiDate; }
            set { aiDate = value;  Id = aiDate.DocIdFromDate(); }
        }


        [JsonProperty(PropertyName = "aiDataList")]
        public List<AIData> AIDataList { get; set; }


    }
}
