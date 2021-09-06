using System;

namespace OpenRCT2.Api.Client.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public DateTime Joined { get; set; }
        public int Comments { get; set; }
        public int Uploads { get; set; }
        public string[] Traits { get; set; }
        public string Avatar { get; set; }
    }
}
