using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.AuthTokens)]
    public class AuthToken
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [SecondaryIndex]
        public string UserId { get; set; }
        [SecondaryIndex]
        public string Token { get; set; }
        public DateTime Created { get;set; }
        public DateTime LastAccessed { get;set; }
    }
}
