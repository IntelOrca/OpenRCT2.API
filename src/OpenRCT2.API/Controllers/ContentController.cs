using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace OpenRCT2.API.Controllers
{
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly UserAuthenticationService _authService;
        private readonly IContentRepository _contentRepository;
        private readonly StorageService _storageService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public ContentController(
            UserAuthenticationService authService,
            IContentRepository contentRepository,
            StorageService storageService,
            IUserRepository userRepository,
            ILogger<ContentController> logger)
        {
            _authService = authService;
            _contentRepository = contentRepository;
            _storageService = storageService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("content/verifyName")]
        public async Task<object> VerifyNameAsync(
            [FromQuery] string owner,
            [FromQuery] string name)
        {
            var err = await VerifyOwnerNameAsync(owner, name);
            return err switch
            {
                ErrorKind.Unauthenticated => Unauthorized(),
                ErrorKind.NoPermissionForOwner => Forbidden(),
                ErrorKind.None => Ok(new
                {
                    Valid = true
                }),
                _ => Ok(new
                {
                    Valid = false,
                    Message = ErrorHandler.GetErrorMessage(err)
                }),
            };
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

            var msg = ValidateImageFileAsync(image);
            if (msg != null)
            {
                return BadRequest(msg);
            }

            var contentType = GetContentTypeAsync(file);
            if (contentType == null)
            {
                return BadRequest("Unsupported content type.");
            }

            if (await VerifyOwnerNameAsync(owner, name) != ErrorKind.None)
            {
                return BadRequest("Invalid owner or name.");
            }

            var ownerUser = await _userRepository.GetUserFromNameAsync(owner);
            if (ownerUser == null)
            {
                return BadRequest("Invalid owner.");
            }

            var imageKey = await UploadImageAsync(image);
            var fileKey = await UploadFileAsync(file, "application/octet-stream", ".json");

            var now = DateTime.UtcNow;
            var contentItem = new ContentItem()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                OwnerId = ownerUser.Id,
                ContentType = 0,
                Image = imageKey,
                File = fileKey,
                Created = now,
                Modified = now
            };
            await _contentRepository.InsertAsync(contentItem);

            return Ok(new
            {
                Owner = owner,
                Name = name
            });
        }

        private async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            _logger.LogInformation("Uploading image to storage...");
            string contentType;
            string extension;
            using (var stream = imageFile.OpenReadStream())
            {
                var format = await SixLabors.ImageSharp.Image.DetectFormatAsync(stream);
                contentType = format.DefaultMimeType;
                extension = format.FileExtensions.First();
            }
            using (var stream = imageFile.OpenReadStream())
            {
                var key = "content/image/" + Guid.NewGuid() + extension;
                await _storageService.UploadPublicFileAsync(stream, key, contentType);
                return key;
            }
        }

        private async Task<string> UploadFileAsync(IFormFile file, string contentType, string extension)
        {
            _logger.LogInformation("Uploading content file to storage...");
            using (var stream = file.OpenReadStream())
            {
                var key = "content/file/" + Guid.NewGuid() + extension;
                await _storageService.UploadPublicFileAsync(stream, key, contentType);
                return key;
            }
        }

        private BadRequestObjectResult BadRequest(string message)
        {
            return BadRequest(new
            {
                Message = message
            });
        }

        private StatusCodeResult Forbidden()
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        private async Task<ErrorKind> VerifyOwnerNameAsync(string owner, string name)
        {
            var user = await _authService.GetAuthenticatedUserAsync();
            if (user == null)
            {
                return ErrorKind.Unauthenticated;
            }

            if (!string.Equals(owner, user.Name))
            {
                return ErrorKind.NoPermissionForOwner;
            }

            if (!IsValidName(name))
            {
                return ErrorKind.NameInvalid;
            }

            var ownerUser = await _userRepository.GetUserFromNameAsync(owner);
            if (ownerUser == null)
            {
                return ErrorKind.OwnerNotFound;
            }

            if (await _contentRepository.ExistsAsync(ownerUser.Id, name))
            {
                return ErrorKind.NameAlreadyUsed;
            }

            return ErrorKind.None;
        }

        private static async Task<string> GetContentTypeAsync(IFormFile file)
        {
            if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using var fs = file.OpenReadStream();
                var doc = await JsonDocument.ParseAsync(fs);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("entries", out var el) && el.ValueKind == JsonValueKind.Array)
                    {
                        return "theme";
                    }
                }
            }
            return null;
        }

        private static async Task<ErrorKind> ValidateImageFileAsync(IFormFile imageFile)
        {
            var (info, format) = await SixLabors.ImageSharp.Image.IdentifyWithFormatAsync(imageFile.OpenReadStream());
            if (format == PngFormat.Instance ||
                format == JpegFormat.Instance)
            {
                if (info.Width > 128 && info.Height > 128 &&
                    info.Width < 2048 && info.Height < 2048)
                {
                    return ErrorKind.None;
                }
                else
                {
                    return ErrorKind.BadImageSize;
                }
            }
            return ErrorKind.BadImageFormat;
        }

        private static bool IsValidName(string name) => name.All(IsValidNameCharacter);

        private static bool IsValidNameCharacter(char c)
        {
            return c switch
            {
                '_' => true,
                '-' => true,
                '.' => true,
                var _ when c >= 'A' && c <= 'Z' => true,
                var _ when c >= 'a' && c <= 'z' => true,
                _ => false
            };
        }
    }

    public static class ErrorHandler
    {
        public static string GetErrorMessage(ErrorKind error)
        {
            return error switch
            {
                ErrorKind.NameInvalid => "Name must only contain A-Z or _ or - or .",
                ErrorKind.NameAlreadyUsed => "Name is already in use.",
                ErrorKind.OwnerNotFound => "Owner not found.",
                ErrorKind.BadImageSize => "Image size must be between 128 and 2048 pixels.",
                ErrorKind.BadImageFormat => "Image must either be png or jpeg format.",
                ErrorKind.UnsupportedContentType => "Unsupported content type.",
                _ => throw new NotImplementedException()
            };
        }
    }

    public enum ErrorKind
    {
        None,
        Unauthenticated,
        Unauthorized,
        NameInvalid,
        NameAlreadyUsed,
        NoPermissionForOwner,
        OwnerNotFound,
        BadImageFormat,
        BadImageSize,
        BadImageFileSize,
        UnsupportedContentType,
    }
}
