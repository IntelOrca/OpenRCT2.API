using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
        public string Name { get; set; }
        public string NameNormalised { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime? LastAuthenticated { get;set; }
        public string Bio { get; set; }

        public int OpenRCT2orgId { get; set; }
    }
}
