using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestAICore.DataObjects
{
    public class Universe : Document
    {
        [JsonProperty(PropertyName = "universeName")]
        public string UniverseName { get; set; }

        [JsonProperty(PropertyName = "constituents")]
        public List<Constituent> Constituents { get; set; }

    }
}
