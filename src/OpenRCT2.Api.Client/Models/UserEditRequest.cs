namespace OpenRCT2.Api.Client.Models
{
    public class UserEditRequest
    {
        public string Name { get; set; }
        public UserAccountStatus? Status { get; set; }
        public string SuspensionReason { get; set; }
        public string EmailCurrent { get; set; }
        public string EmailNew { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
    }
}
