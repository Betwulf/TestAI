using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class Constituent
    {
        [JsonProperty(PropertyName = "ticker")]
        public string Ticker { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "sector")]
        public string Sector { get; set; }

        // // Add this back later for anonymity on ML
        //[JsonProperty(PropertyName = "ConstituentNumber")]
        //public int ConstituentNumber { get; set; }
    }
}
