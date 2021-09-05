namespace OpenRCT2.API.Models.Requests
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Captcha { get; set; }
    }
}
