using System.ComponentModel.DataAnnotations;

namespace OpenRCT2.API.Models.Requests
{
    public class SignInRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
