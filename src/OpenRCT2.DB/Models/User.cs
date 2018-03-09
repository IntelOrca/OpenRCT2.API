using System;
using Newtonsoft.Json;

namespace OpenRCT2.DB.Models
{
    [Table(TableNames.Users)]
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
        public string Name { get; set; }
        [SecondaryIndex]
        public string NameNormalised { get; set; }
        [SecondaryIndex]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime? LastAuthenticated { get;set; }
        public string Bio { get; set; }
        public DateTime? EmailVerified { get; set; }
        public string EmailVerifyToken { get; set; }

        [SecondaryIndex]
        public int OpenRCT2orgId { get; set; }
    }
}
