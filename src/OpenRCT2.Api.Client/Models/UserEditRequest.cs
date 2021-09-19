namespace OpenRCT2.Api.Client.Models
{
    public class UserEditRequest
    {
        public string Name { get; set; }
        public UserAccountStatus? Status { get; set; }
        public string NewEmail { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
    }
}
