using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Implementations;

namespace OpenRCT2.API.Controllers
{
    [Route("build")]
    public class BuildController : Controller
    {
        private const string LatestUrl = "https://openrct2.org/downloads/develop/latest";
        private const string SpecificUrl = "https://openrct2.org/downloads/develop/{0}";

        private static (DateTime, object)? _cachedLatest;

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BuildController(HttpClient httpClient, ILogger<BuildController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("latest")]
        public async Task<object> GetLatestAsync()
        {
            // Cache latest build to reduce requests to openrct2.org
            object latestBuilds = null;
            if (_cachedLatest.HasValue)
            {
                var (dt, cachedValue) = _cachedLatest.Value;
                if ((DateTime.UtcNow - dt).TotalMinutes < 1)
                {
                    latestBuilds = cachedValue;
                }
            }
            if (latestBuilds == null)
            {
                latestBuilds = await GetBuildsAsync(LatestUrl).ConfigureAwait(false);
                if (latestBuilds == null)
                {
                    return NotFound(JResponse.Error("Build not found"));
                }
                _cachedLatest = (DateTime.UtcNow, latestBuilds);
            }
            return new
            {
                Status = JStatus.OK,
                Result = latestBuilds
            };
        }

        [HttpGet("{commit}")]
        public async Task<object> GetAsync(string commit)
        {
            var url = string.Format(SpecificUrl, commit);
            var latestBuilds = await GetBuildsAsync(url).ConfigureAwait(false);
            if (latestBuilds == null)
            {
                return NotFound(JResponse.Error("Build not found"));
            }
            return new
            {
                Status = JStatus.OK,
                Result = latestBuilds
            };
        }

        private static readonly Regex AvailableSinceRegex = new Regex(
            @"Available since: (\d\d\d\d)-(\d\d)-(\d\d) (\d\d):(\d\d):(\d\d)",
            RegexOptions.Compiled);
        private static readonly Regex DownloadUrlRegex = new Regex(
            @"""(http://cdn.limetric.com/games/openrct2/([0-9.]+)/([a-z]+)/([0-9a-z]+)/(\d+)/(.+))""",
            RegexOptions.Compiled);

        private async Task<object> GetBuildsAsync(string url)
        {
            // Example download URL that we are trying to find:
            // http://cdn.limetric.com/games/openrct2/0.1.2/develop/2c6804f/1/OpenRCT2-0.1.2-develop-2c6804f-windows-win32.zip
            _logger.LogInformation($"Downloading page: {url}");
            var html = await _httpClient.GetStringAsync(url);
            if (html.Contains("<title>Downloads - OpenRCT2 project</title>"))
            {
                // Specific build was not found
                return null;
            }

            DateTime? date = null;
            var dateMatch = AvailableSinceRegex.Match(html);
            if (dateMatch != null)
            {
                try
                {
                    var values = dateMatch.Groups
                        .Skip(1)
                        .Select(x => Int32.Parse(x.Value))
                        .ToArray();
                    date = new DateTime(values[0], values[1], values[2], values[3], values[4], values[5], DateTimeKind.Utc);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to parse `Available since` text.");
                }
            }

            var matches = DownloadUrlRegex.Matches(html);
            var results = new List<object>();
            foreach (Match match in matches)
            {
                var values = match.Groups
                    .Select(x => x.Value)
                    .ToArray();
                results.Add(
                    new
                    {
                        Download = values[1],
                        Version = values[2],
                        Branch = values[3],
                        CommitShort = values[4],
                        Flavour = values[5],
                        FileName = values[6]
                    });
            }
            return new
            {
                date,
                builds = results
            };
        }
    }
}
