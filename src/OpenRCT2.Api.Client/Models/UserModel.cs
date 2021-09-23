using System;

namespace OpenRCT2.Api.Client.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string EmailPending { get; set; }
        public UserAccountStatus Status { get; set; }
        public string SecretKey { get; set; }
        public string Bio { get; set; }
        public DateTime Joined { get; set; }
        public string AvatarUrl { get; set; }

        public bool CanEdit { get; set; }
    }
}
