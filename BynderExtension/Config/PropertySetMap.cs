using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bynder.Config
{
    public class PropertySetMap
    {
        [JsonProperty("inRiverFieldId")]
        public string InRiverFieldId { get; set; }

        [JsonProperty("cvlMapping")]
        public Dictionary<string,string> CvlMapping { get; set; }
        [JsonProperty("culture")]
        public string Culture { get; set; }
    }
}
