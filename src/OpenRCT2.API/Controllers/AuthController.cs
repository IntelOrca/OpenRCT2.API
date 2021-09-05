using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly IAuthTokenRepository _authTokenRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AuthenticationService authService,
            IAuthTokenRepository authTokenRepository,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _authTokenRepository = authTokenRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<object> PostAsync(Body body)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                // Restrict API to only offical clients
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var user = await _authService.GetAuthenticatedUserAsync();
            if (user != null)
            {
                // For now, restrict authenticated users from generating tokens
                return BadRequest();
            }

            user = await _authService.AuthenticateAsync(body.Email, body.Password);
            if (user == null || user.Status != AccountStatus.Active)
            {
                return Unauthorized();
            }

            var token = GenerateToken();
            var dt = DateTime.UtcNow;
            await _authTokenRepository.InsertAsync(new AuthToken() {
                Id = user.Id,
                Token = token,
                Created = dt,
                LastAccessed = dt
            });

            _logger.LogInformation("A new authentication token was generated for User #{0}", user.Id);

            return Ok(new
            {
                UserId = user.Id,
                user.Name,
                Token = token
            });
        }

        [HttpDelete]
        public async Task<object> DeleteAsync(Body body)
        {
            if (!await _authService.IsClientAuthEnabledAsync())
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var tokenOwner = _authTokenRepository.GetFromTokenAsync(body.Token);
            if (tokenOwner == null)
            {
                return NotFound();
            }

            // TODO right now there is no check for token ownership or authentication,
            //      should there be?

            await _authTokenRepository.DeleteAsync(body.Token);

            _logger.LogInformation("An authentication token was revoked for User #{0}", tokenOwner.Id);

            // Revoke token
            return Ok();
        }

        private static string GenerateToken()
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[48];
            rng.GetBytes(bytes);
            var token = Convert.ToBase64String(bytes);
            Array.Clear(bytes, 0, bytes.Length);
            return token;
        }

        public class Body
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }
        }
    }
}
