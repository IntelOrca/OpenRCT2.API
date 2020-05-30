using System.Text.Json.Serialization;

namespace OpenRCT2.API.JsonModels
{
    public class ServerAddressList
    {
        [JsonPropertyName("v4")]
        public string[] IPv4 { get; set; }
        [JsonPropertyName("v6")]
        public string[] IPv6 { get; set; }
    }
}
