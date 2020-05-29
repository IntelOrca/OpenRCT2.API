using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Implementations;

namespace OpenRCT2.API.Controllers
{
    [Route("build")]
    public class BuildController : Controller
    {
        private const string DownloadsUrl = "https://openrct2.org/downloads";
        private const string LatestUrl = "https://openrct2.org/downloads/develop/latest";
        private const string SpecificUrl = "https://openrct2.org/downloads/develop/{0}";

        private static TimeSpan _cacheTime = TimeSpan.FromMinutes(5);
        private static (DateTime, object)? _cachedLatest;

        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BuildController(HttpClient httpClient, IWebHostEnvironment env, ILogger<BuildController> logger)
        {
            _httpClient = httpClient;
            if (!env.IsProduction())
            {
                _cacheTime = TimeSpan.FromSeconds(10);
            }
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
                if (DateTime.UtcNow - dt < _cacheTime)
                {
                    latestBuilds = cachedValue;
                }
            }
            if (latestBuilds == null)
            {
                latestBuilds = await GetLatestBuildsAsync();
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
            var latestBuilds = await GetBuildsAsync(url);
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
        private static readonly Regex CommitPageRegex = new Regex(
            @"""/downloads/develop/([a-z0-9]+)""",
            RegexOptions.Compiled);

        private readonly int[] RequiredFlavours = new int[] { 1, 2, 6, 7, 3, 9, 4 };

        private async Task<object> GetLatestBuildsAsync()
        {
            DateTime? date = null;
            var flavoursLeftToFind = new List<int>(RequiredFlavours);
            var totalBuilds = new Dictionary<int, BuildInfo>();

            var recentBuilds = await GetRecentBuildCommitsAsync();
            foreach (var commit in recentBuilds)
            {
                var url = string.Format(SpecificUrl, commit);
                var builds = await GetBuildsAsync(url);
                foreach (var b in builds)
                {
                    if (!date.HasValue)
                    {
                        date = b.Date;
                    }
                    if (!totalBuilds.ContainsKey(b.Flavour))
                    {
                        totalBuilds[b.Flavour] = b;
                        flavoursLeftToFind.Remove(b.Flavour);
                    }
                }
                if (flavoursLeftToFind.Count == 0)
                {
                    // All required flavours found, stop searching through recent builds
                    break;
                }
            }

            return new
            {
                date,
                builds = totalBuilds.Values
            };
        }

        private async Task<List<BuildInfo>> GetBuildsAsync(string url)
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
                        .OfType<Group>()
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
            var results = new List<BuildInfo>();
            foreach (Match match in matches)
            {
                var values = match.Groups
                    .OfType<Group>()
                    .Select(x => x.Value)
                    .ToArray();
                results.Add(
                    new BuildInfo()
                    {
                        Date = date,
                        Download = values[1],
                        Version = values[2],
                        Branch = values[3],
                        CommitShort = values[4],
                        Flavour = Int32.Parse(values[5]),
                        FileName = values[6]
                    });
            }
            return results;
        }

        private async Task<string[]> GetRecentBuildCommitsAsync()
        {
            _logger.LogInformation($"Downloading page: {DownloadsUrl}");
            var html = await _httpClient.GetStringAsync(DownloadsUrl);
            var matches = CommitPageRegex.Matches(html);
            return matches
                .Select(x => x.Groups[1].Value)
                .ToArray();
        }

        private class BuildInfo
        {
            [JsonIgnore]
            public DateTime? Date { get; set; }
            public string Download { get; set; }
            public string Version { get; set; }
            public string Branch { get; set; }
            public string CommitShort { get; set; }
            public int Flavour { get; set; }
            public string FileName { get; set; }
        }
    }
}
