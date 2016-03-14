namespace OpenRCT2.API.Models
{
    public class User
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string AuthenticationSalt { get; set; }
    }
}
