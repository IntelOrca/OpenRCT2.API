using Newtonsoft.Json;

namespace OpenRCT2.API.JsonModels
{
    public class ServerAddressList
    {
        [JsonProperty("v4")]
        public string[] IPv4 { get; set; }
        [JsonProperty("v6")]
        public string[] IPv6 { get; set; }
    }
}
