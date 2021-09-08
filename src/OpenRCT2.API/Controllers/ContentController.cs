using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Services;

namespace OpenRCT2.API.Controllers
{
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly UserAuthenticationService _authService;

        public ContentController(UserAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("content/upload")]
        public async Task<object> PostAsync(
            [FromForm] string owner,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string visibility,
            [FromForm] IFormFile file,
            [FromForm] IFormFile image)
        {
            // Check user has owner
            var user = await _authService.GetAuthenticatedUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ValidateImageFile(image))
            {
                return BadRequest(new
                {
                    Message = "Unsupported image format."
                });
            }

            var contentType = GetContentType(file);
            if (contentType == null)
            {
                return BadRequest(new
                {
                    Message = "Unsupported content type."
                });
            }

            return Ok(new
            {
                Owner = owner,
                Name = name
            });
        }

        private string GetContentType(IFormFile file)
        {
            return "unknown";
        }

        private bool ValidateImageFile(IFormFile image)
        {
            return true;
        }
    }
}
