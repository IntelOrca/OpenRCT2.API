using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
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

        [HttpGet("content/{owner}/{name}")]
        public async Task<object> GetAsync(string owner, string name)
        {
            var currentUser = await _authService.GetAuthenticatedUserAsync();
            var user = await _userRepository.GetUserFromNameAsync(owner);
            if (user != null)
            {
                var canEdit = currentUser.Id == user.Id;
                var content = await _contentRepository.GetAsync(user.Id, name);
                if (content != null)
                {
                    return Ok(GetContentResponse(user.Name, content, canEdit));
                }
            }
            return NotFound();
        }

        [HttpGet("content")]
        public async Task<object> GetAsync(
            [FromQuery] string owner)
        {
            if (owner != null)
            {
                var currentUser = await _authService.GetAuthenticatedUserAsync();
                var user = await _userRepository.GetUserFromNameAsync(owner);
                if (user != null)
                {
                    var canEdit = currentUser?.Id == user.Id;
                    var content = await _contentRepository.GetAllAsync(new ContentQuery()
                    {
                        OwnerId = user.Id,
                        SortBy = ContentSortKind.DateCreated
                    });
                    return content.Select(x => GetContentResponse(user.Name, x, canEdit));
                }
            }
            return Array.Empty<object>();
        }

        [HttpGet("content/recent")]
        public Task<object> GetRecentAsync() => GetAllAsync(ContentSortKind.DateCreated);

        [HttpGet("content/popular")]
        public Task<object> GetPopularAsync() => GetAllAsync(ContentSortKind.Popularity);

        private async Task<object> GetAllAsync(ContentSortKind sortKind)
        {
            var currentUser = await _authService.GetAuthenticatedUserAsync();
            var currentUserId = currentUser?.Id;
            var isAdmin = currentUser?.Status == AccountStatus.Administrator;

            var content = await _contentRepository.GetAllAsync(new ContentQuery()
            {
                SortBy = sortKind
            });
            return content.Select(x => GetContentResponse(x, currentUserId, isAdmin));
        }

        private object GetContentResponse(ContentItemExtended content, string currentUserId, bool isAdmin) =>
            GetContentResponse(content.Owner, content, isAdmin || content.OwnerId == currentUserId);

        private object GetContentResponse(string owner, ContentItem content, bool canEdit = false)
        {
            return new
            {
                Owner = owner,
                Name = content.Name,
                Title = content.Title,
                Description = content.Description,
                ImageUrl = _storageService.GetPublicUrl(content.ImageKey),
                FileUrl = _storageService.GetPublicUrl(content.FileKey),
                Visibility = content.Visibility.ToString().ToLowerInvariant(),
                Created = content.Created,
                Modified = content.Modified,
                CanEdit = canEdit
            };
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

            var err = await ValidateImageFileAsync(image);
            if (err != ErrorKind.None)
            {
                return ErrorResponse(err);
            }

            var contentType = await GetContentTypeAsync(file);
            if (contentType == 0)
            {
                return ErrorResponse(ErrorKind.UnsupportedContentType);
            }

            err = await VerifyOwnerNameAsync(owner, name);
            if (err != ErrorKind.None)
            {
                return ErrorResponse(err);
            }

            var ownerUser = await _userRepository.GetUserFromNameAsync(owner);
            if (ownerUser == null)
            {
                return ErrorResponse(ErrorKind.OwnerNotFound);
            }

            var contentVisibility = ParseVisibility(visibility);
            if (contentVisibility == null)
            {
                return ErrorResponse(ErrorKind.InvalidContentVisibility);
            }

            var contentId = Guid.NewGuid().ToString();
            _logger.LogInformation("Inserting new content {0} ({1}/{2}) for user {3}", contentId, owner, name, user.Id);
            try
            {
                // Upload the files first
                await using var imageUploadTransaction = await UploadImageAsync(image, contentId);
                await using var fileUploadTransaction = await UploadFileAsync(file, "application/octet-stream", file.FileName.ToLowerInvariant(), contentId);

                // Now insert the database entry
                var now = DateTime.UtcNow;
                var contentItem = new ContentItem()
                {
                    Id = contentId,
                    Name = name,
                    NormalisedName = name.ToLowerInvariant(),
                    OwnerId = ownerUser.Id,
                    ContentType = 1,
                    ImageKey = imageUploadTransaction.Key,
                    FileKey = fileUploadTransaction.Key,
                    Created = now,
                    Modified = now,
                    Description = description,
                    Visibility = contentVisibility.Value
                };
                await _contentRepository.InsertAsync(contentItem);

                // Finalise
                imageUploadTransaction.Commit();
                fileUploadTransaction.Commit();
                _logger.LogInformation("New content {0} ({1}/{2}) for user {3} successful", contentId, owner, name, user.Id);

                return Ok(new
                {
                    Valid = true,
                    Owner = owner,
                    Name = name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "New content {0} ({1}/{2}) for user {3} failed", contentId, owner, name, user.Id);
                throw;
            }
        }

        [HttpPut("content/{rOwner}/{rName}")]
        public async Task<object> PutAsync(
            [FromRoute] string rOwner,
            [FromRoute] string rName,
            [FromForm] string name,
            [FromForm] string title,
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

            // Get current content item
            var ownerUser = await _userRepository.GetUserFromNameAsync(rOwner);
            if (ownerUser == null)
            {
                return NotFound();
            }
            var contentItem = await _contentRepository.GetAsync(ownerUser.Id, rName);

            // Check user edit this content
            if (user.Id != ownerUser.Id && user.Status != AccountStatus.Administrator)
            {
                if (contentItem.Visibility == ContentVisibility.Private)
                {
                    return NotFound();
                }
                else
                {
                    return Forbidden();
                }
            }

            // Check for rename
            var normalisedName = name.ToLowerInvariant();
            if (normalisedName != contentItem.NormalisedName)
            {
                // We need to validate the new name
                var err = await VerifyOwnerNameAsync(ownerUser.Name, name);
                if (err != ErrorKind.None)
                {
                    return ErrorResponse(err);
                }
            }
            // Apply either new name or new casing of name
            contentItem.Name = name;
            contentItem.NormalisedName = normalisedName;

            contentItem.Title = title;
            contentItem.Description = description;

            var contentVisibility = ParseVisibility(visibility);
            if (contentVisibility == null)
            {
                return ErrorResponse(ErrorKind.InvalidContentVisibility);
            }
            contentItem.Visibility = contentVisibility.Value;
            contentItem.Modified = DateTime.UtcNow;

            // Validate image
            if (image != null)
            {
                var imageError = await ValidateImageFileAsync(image);
                if (imageError != ErrorKind.None)
                {
                    return ErrorResponse(imageError);
                }
            }

            // Validate content type
            if (file != null)
            {
                var contentType = await GetContentTypeAsync(file);
                if (contentType != contentItem.ContentType)
                {
                    return ErrorResponse(ErrorKind.UnsupportedContentType);
                }
            }

            _logger.LogInformation("Updating content {0} ({1}/{2}) for user {3}", contentItem.Id, ownerUser.Name, name, user.Id);
            StorageService.Transaction imageUploadTransaction = null;
            StorageService.Transaction fileUploadTransaction = null;
            try
            {
                var oldImageKey = contentItem.ImageKey;
                var oldFileKey = contentItem.FileKey;

                // Upload the files first
                if (image != null)
                {
                    imageUploadTransaction = await UploadImageAsync(image, contentItem.Id);
                    contentItem.ImageKey = imageUploadTransaction.Key;
                }

                if (file != null)
                {
                    fileUploadTransaction = await UploadFileAsync(file, "application/octet-stream", file.FileName.ToLowerInvariant(), contentItem.Id);
                    contentItem.FileKey = fileUploadTransaction.Key;
                }

                // Now update the database entry
                var now = DateTime.UtcNow;
                await _contentRepository.UpdateAsync(contentItem);

                // Finalise
                imageUploadTransaction?.Commit();
                fileUploadTransaction?.Commit();
                _logger.LogInformation("Update content {0} ({1}/{2}) for user {3} successful", contentItem.Id, ownerUser.Name, name, user.Id);

                // Delete old files
                _logger.LogInformation("Deleting content's old files {0} ({1}/{2}) for user {3} successful", contentItem.Id, ownerUser.Name, name, user.Id);
                if (image != null)
                {
                    _storageService.DeleteAsync(oldImageKey).Forget();
                }
                if (file != null)
                {
                    _storageService.DeleteAsync(oldFileKey).Forget();
                }

                return Ok(new
                {
                    Valid = true,
                    Owner = ownerUser.Name,
                    Name = name
                });
            }
            catch (Exception ex)
            {
                await imageUploadTransaction.DisposeAsync();
                await fileUploadTransaction.DisposeAsync();

                _logger.LogError(ex, "New content {0} ({1}/{2}) for user {3} failed", contentItem.Id, ownerUser.Name, name, user.Id);
                throw;
            }
        }

        private async Task<StorageService.Transaction> UploadImageAsync(IFormFile imageFile, string contentId)
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
                var key = "content/image/" + Guid.NewGuid() + "/preview." + extension;
                var tags = new[] { new KeyValuePair<string, string>("contentId", contentId) };
                return await _storageService.UploadPublicFileTransactionAsync(stream, key, contentType, tags);
            }
        }

        private async Task<StorageService.Transaction> UploadFileAsync(IFormFile file, string contentType, string fileName, string contentId)
        {
            _logger.LogInformation("Uploading content file to storage...");
            using (var stream = file.OpenReadStream())
            {
                var key = "content/file/" + Guid.NewGuid() + "/" + fileName;
                var tags = new[] { new KeyValuePair<string, string>("contentId", contentId) };
                return await _storageService.UploadPublicFileTransactionAsync(stream, key, contentType, tags);
            }
        }

        private ActionResult ErrorResponse(ErrorKind err)
        {
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

        private static async Task<int> GetContentTypeAsync(IFormFile file)
        {
            if (file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using var fs = file.OpenReadStream();
                var doc = await JsonDocument.ParseAsync(fs);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("entries", out var el) && el.ValueKind == JsonValueKind.Object)
                    {
                        return 1;
                    }
                }
            }
            return 0;
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

        private static ContentVisibility? ParseVisibility(string s)
        {
            return Enum.TryParse<ContentVisibility>(s, out var value) ? value : null;
        }
    }

    public static class ErrorHandler
    {
        public static string GetErrorMessage(ErrorKind error)
        {
            return error switch
            {
                ErrorKind.None or
                ErrorKind.Unauthenticated or
                ErrorKind.Unauthorized or
                ErrorKind.NoPermissionForOwner => throw new ArgumentException(null, nameof(error)),
                ErrorKind.NameInvalid => "Name must only contain A-Z or _ or - or .",
                ErrorKind.NameAlreadyUsed => "Name is already in use.",
                ErrorKind.OwnerNotFound => "Owner not found.",
                ErrorKind.BadImageSize => "Image size must be between 128 and 2048 pixels.",
                ErrorKind.BadImageFileSize => "Image file size must be less than 8 MiB.",
                ErrorKind.BadImageFormat => "Image must either be png or jpeg format.",
                ErrorKind.UnsupportedContentType => "Unsupported content type.",
                ErrorKind.InvalidContentVisibility => "Invalid content visibility",
                ErrorKind.IncorrectContentType => "Incorrect content type",
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
        InvalidContentVisibility,
        IncorrectContentType,
    }
}
