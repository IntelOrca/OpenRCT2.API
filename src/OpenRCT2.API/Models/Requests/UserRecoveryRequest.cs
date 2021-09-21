namespace OpenRCT2.API.Models.Requests
{
    public class UserRecoveryRequest
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public string PasswordHash { get; set; }
    }
}
