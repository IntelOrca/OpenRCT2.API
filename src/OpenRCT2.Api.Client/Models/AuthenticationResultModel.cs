namespace OpenRCT2.Api.Client.Models
{
    public class AuthenticationResultModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Token { get; set; }
    }
}
