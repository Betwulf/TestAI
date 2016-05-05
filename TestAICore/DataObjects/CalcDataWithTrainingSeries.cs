using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace TestAICore.DataObjects
{
    public class CalcDataWithTrainingSeries : Document
    {
        public CalcDataWithTrainingSeries()
        {
            CalcDataList = new List<CalcDataWithTraining>();
        }

        

        [JsonProperty(PropertyName = "calcDataList")]
        public List<CalcDataWithTraining> CalcDataList { get; set; }

    }
}
