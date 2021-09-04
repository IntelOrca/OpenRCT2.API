using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Services
{
    public class NeDesignsService
    {
        private static readonly TimeSpan _queryWait = TimeSpan.FromHours(6);

        private readonly IRctObjectRepository _rctObjectRepo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NeDesignsService> _logger;
        private DateTime _lastQueryCheck;
        private AsyncSemaphore _querySemaphore = new AsyncSemaphore(1);

        public NeDesignsService(IRctObjectRepository rctObjectRepo, HttpClient httpClient, ILogger<NeDesignsService> logger)
        {
            _rctObjectRepo = rctObjectRepo;
            _httpClient = httpClient;
            _logger = logger;
        }

        public string GetUrl(int nedesignsId, string name)
        {
            return $"https://www.nedesigns.com/rct2-object/{nedesignsId}/{name.ToLowerInvariant()}/download/";
        }

        public bool HasEnoughTimePassedToQuery() => DateTime.UtcNow - _lastQueryCheck > _queryWait;

        public async Task SearchForNewObjectsAsync()
        {
            // Do not allow more than one thread to do this concurrently
            using (await _querySemaphore.EnterAsync())
            {
                _lastQueryCheck = DateTime.UtcNow;

                // Find maximum NeDesigns ID
                var obj = await _rctObjectRepo.GetLegacyObjectWithHighestNeIdAsync();
                var neId = obj == null ? 1 : obj.NeDesignId;
                var fails = 0;
                while (fails < 3)
                {
                    neId++;
                    try
                    {
                        _logger.LogInformation($"Querying NEDesigns for object id: {neId}");
                        var queryUrl = $"https://www.nedesigns.com/rct2-object/{neId}/x/";
                        var queryHtml = await _httpClient.GetStringAsync(queryUrl);
                        var match = Regex.Match(queryHtml, $"/rct2-object/{neId}/(.+)/download/");
                        if (match.Success)
                        {
                            var name = match.Groups[1].Value.ToUpperInvariant();
                            await _rctObjectRepo.UpdateLegacyAsync(
                                new LegacyRctObject() { NeDesignId = neId, Name = name });
                            _logger.LogInformation($"Adding new object: #{neId} [{name}]");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to query object {neId}");
                        fails++;
                    }
                }
            }
        }
    }
}
