using System;

namespace OpenRCT2.API.OpenRCT2org
{
    public class JUser
    {
        public int userId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool isVerified { get; set; }
        public bool isBanned { get; set; }
        public DateTime joined { get; set; }
        public DateTime lastActivity { get; set; }
        public DateTime lastVisit { get; set; }
        public string seoName { get; set; }
        public string profileUrl { get; set; }
    }
}
