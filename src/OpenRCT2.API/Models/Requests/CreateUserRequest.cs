using System.ComponentModel.DataAnnotations;

namespace OpenRCT2.API.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Captcha { get; set; }
    }
}
