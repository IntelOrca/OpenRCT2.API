using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenRCT2.API.Configuration;

namespace OpenRCT2.API.Services
{
    public class GoogleRecaptchaService
    {
        private readonly string _secret;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public GoogleRecaptchaService(IOptions<ApiConfig> config, HttpClient httpClient, ILogger<GoogleRecaptchaService> logger)
        {
            _secret = config.Value.ReCaptchaSecret;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> ValidateAsync(string remoteIp, string token)
        {
            _logger.LogInformation($"Validating reCaptcha for {remoteIp}");
            if (string.IsNullOrEmpty(_secret))
            {
                _logger.LogWarning("No reCapatcha key set, no validation performed.");
                return true;
            }
            else
            {
                var data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _secret),
                    new KeyValuePair<string, string>("response", token),
                    new KeyValuePair<string, string>("remoteIp", remoteIp),
                });
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", data).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ReCaptchaValidateResponse>(content);
                if (result.Success == "true")
                {
                    return true;
                }
                else
                {
                    var errorReason = result.ErrorCodes == null ? "unknown" : $"[{string.Join(",", result.ErrorCodes)}]";
                    _logger.LogWarning($"reCapatcha failed for {remoteIp}. Reason: {errorReason}");
                    return false;
                }
            }
        }

        private class ReCaptchaValidateResponse
        {
            public string Success { get; set; }
            [JsonProperty("challenge_ts")]
            public string ChallengeTs { get; set; }
            public string Hostname { get; set; }
            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }
    }
}
