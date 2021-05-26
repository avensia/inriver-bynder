using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bynder.Config
{
    public class PropertySetMap
    {
        public PropertySetMap()
        {
            CvlMapping = new Dictionary<string, string>();
        }

        [JsonProperty("inRiverFieldId")]
        public string InRiverFieldId { get; set; }

        [JsonProperty("cvlMapping")]
        public Dictionary<string,string> CvlMapping { get; set; }
        [JsonProperty("culture")]
        public string Culture { get; set; }
    }
}
