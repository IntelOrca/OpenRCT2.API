using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.AppVeyor;

namespace OpenRCT2.API.Implementations
{
    public class LocalisationService : ILocalisationService
    {
        private const string AppVeyorAccount = "intelorca";
        private const string AppVeyorProject = "localisation";
        private const string AppVeyorBranch = "master";

        private readonly ILogger<LocalisationService> _logger;
        private readonly IAppVeyorService _appVeyorService;
        private DateTime _lastCheckTime;
        private string _cachedJobId;
        private List<LanguageProgress> _languageProgressList;

        public LocalisationService(ILogger<LocalisationService> logger, IAppVeyorService appVeyorService)
        {
            _logger = logger;
            _appVeyorService = appVeyorService;
        }

        public async Task<int> GetLanguageProgressAsync(string languageId)
        {
            IList<LanguageProgress> languageProgressList = await GetLanguageProgressAsync();
            if (languageProgressList == null)
            {
                return 0;
            }

            LanguageProgress languageProgress = languageProgressList.FirstOrDefault(x => x.LanguageId == languageId);
            if (languageProgress == null)
            {
                return 0;
            }

            return languageProgress.Progress;
        }

        private async Task<IList<LanguageProgress>> GetLanguageProgressAsync()
        {
            try
            {
                if (ShouldReQuery())
                {
                    _lastCheckTime = DateTime.UtcNow;
                    await UpdateLanguageProgressListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(), ex, ex.Message);
            }
            return _languageProgressList;
        }

        private bool ShouldReQuery()
        {
            if (_languageProgressList == null)
            {
                return true;
            }

            TimeSpan span = DateTime.UtcNow - _lastCheckTime;
            return span.TotalMinutes > 5;
        }

        private async Task UpdateLanguageProgressListAsync()
        {
            string mostRecentJobId = await _appVeyorService.GetLastBuildJobIdAsync(AppVeyorAccount,
                                                                                   AppVeyorProject,
                                                                                   AppVeyorBranch);
            if (mostRecentJobId != _cachedJobId)
            {
                JMessage[] messages = await _appVeyorService.GetMessagesAsync(mostRecentJobId);
                _languageProgressList = messages.Select(x => ParseAppVeyorMessage(x))
                                                .Where(x => x != null)
                                                .ToList();
                _languageProgressList.Add(new LanguageProgress("en-GB", 100));

                _cachedJobId = mostRecentJobId;
            }
        }

        private LanguageProgress ParseAppVeyorMessage(JMessage message)
        {
            try
            {
                Match match = Regex.Match(message.message, @"([a-zA-Z-]+):.+\((\d+)%\).+");
                string languageId = match.Groups[1].Value;
                int percent = Int32.Parse(match.Groups[2].Value);
                return new LanguageProgress(languageId, percent);
            }
            catch
            {
                return null;
            }
        }

        private class LanguageProgress
        {
            public string LanguageId { get; }
            public int Progress { get; }

            public LanguageProgress(string languageId, int progress)
            {
                LanguageId = languageId;
                Progress = progress;
            }

            public override string ToString()
            {
                return $"{LanguageId} ({Progress}%)";
            }
        }
    }
}
