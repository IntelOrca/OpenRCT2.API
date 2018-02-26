using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    public class AuthToken
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime Created { get;set; }
        public DateTime LastAccessed { get;set; }
    }
}
