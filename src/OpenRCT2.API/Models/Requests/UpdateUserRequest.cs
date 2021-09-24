using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Models.Requests
{
    public class UpdateUserRequest
    {
        public string Name { get; set; }
        public AccountStatus? Status { get; set; }
        public string SuspensionReason { get; set; }
        public string EmailCurrent { get; set; }
        public string EmailNew { get; set; }
        public string PasswordHash { get; set; }
        public string Bio { get; set; }
    }
}
