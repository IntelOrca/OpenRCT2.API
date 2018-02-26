using System.ComponentModel.DataAnnotations;

namespace OpenRCT2.API.Models.Requests
{
    public class CreateUserRequest
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string Captcha { get; set; }
    }
}
