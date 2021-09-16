using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Threading;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.Services
{
    public class RateLimiterService
    {
        private readonly ConcurrentDictionary<IPAddress, List<DownloadEntry>> _ipToDownload = new();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private DateTime _lastCull = DateTime.UtcNow;

        public RateLimiterService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Records a download for the IP address. Returns true if no download within the last hour
        /// from the same IP address.
        /// </summary>
        /// <returns></returns>
        public ValueTask<bool> RecordDownloadAsync(string contentId)
        {
            var currentIpAddress = _httpContextAccessor.HttpContext.GetRemoteIPAddress();
            var entries = _ipToDownload.GetOrAdd(currentIpAddress, key => new List<DownloadEntry>());
            lock (entries)
            {
                CullRecords(entries);
                if (!entries.Any(x => x.ContentId == contentId))
                {
                    entries.Add(new DownloadEntry(contentId));
                    return ValueTask.FromResult(true);
                }
            }

            CullRecordsIfCullingTimeAsync().Forget();

            return ValueTask.FromResult(false);
        }

        private ValueTask CullRecordsIfCullingTimeAsync()
        {
            if (_lastCull < DateTime.UtcNow - TimeSpan.FromHours(4))
            {
                return new ValueTask(Task.Run(CullRecords));
            }
            return ValueTask.CompletedTask;
        }

        private void CullRecords()
        {
            foreach (var item in _ipToDownload)
            {
                var entries = item.Value;
                lock (entries)
                {
                    CullRecords(entries);
                    if (entries.Count == 0)
                    {
                        _ipToDownload.TryRemove(item);
                    }
                }
            }
        }

        private static void CullRecords(List<DownloadEntry> entries)
        {
            // Remove all download records that are over an hour old
            var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
            entries.RemoveAll(x => x.Time < cutoff);
        }

        private struct DownloadEntry
        {
            public string ContentId { get; }
            public DateTime Time { get; }

            public DownloadEntry(string contentId)
            {
                ContentId = contentId;
                Time = DateTime.UtcNow;
            }
        }
    }
}
