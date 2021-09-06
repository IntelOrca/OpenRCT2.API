using System;
using Microsoft.AspNetCore.Components;

namespace OpenRCT2.Content.Components
{
    public partial class Time
    {
        private string _title;
        private string _friendlyTimeAgo;

        [Parameter]
        public DateTime DateTime { get; set; }

        protected override void OnInitialized()
        {
            _title = DateTime.ToString("g");
            _friendlyTimeAgo = GetRelativeTime(DateTime.UtcNow, DateTime);
        }

        private struct FormatDesc
        {
            public float Edge;
            public string Text;
            public long Other;

            public FormatDesc(float edge, string text, long other)
            {
                Edge = edge;
                Text = text;
                Other = other;
            }
        }

        private const long SECOND = 1;
        private const long MINUTE = 60 * SECOND;
        private const long HOUR = 60 * MINUTE;
        private const long DAY = 24 * HOUR;
        private const long WEEK = 7 * DAY;
        private const long YEAR = DAY * 365;
        private const long MONTH = YEAR / 12;

        private static readonly FormatDesc[] _formats = new[] {
            new FormatDesc(0.7f * MINUTE, "just now", 0),
            new FormatDesc(1.5f * MINUTE, "a minute ago", 0),
            new FormatDesc(60 * MINUTE, "minutes ago", MINUTE),
            new FormatDesc(1.5f * HOUR, "an hour ago", 0),
            new FormatDesc(DAY, "hours ago", HOUR),
            new FormatDesc(2 * DAY, "yesterday", 0),
            new FormatDesc(7 * DAY, "days ago", DAY),
            new FormatDesc(1.5f * WEEK, "a week ago", 0),
            new FormatDesc(MONTH, "weeks ago", WEEK),
            new FormatDesc(1.5f * MONTH, "a month ago", 0),
            new FormatDesc(YEAR, "months ago", MONTH),
            new FormatDesc(1.5f * YEAR, "a year ago", 0),
            new FormatDesc(float.MaxValue, "years ago", YEAR)
        };

        private static string GetRelativeTime(DateTime input, DateTime reference)
        {
            var delta = (input - reference).TotalSeconds;
            for (var i = 0; i < _formats.Length; i++)
            {
                var format = _formats[i];
                if (delta < format.Edge)
                {
                    return format.Other == 0 ? format.Text : Math.Round(delta / format.Other) + " " + format.Text;
                }
            }
            return string.Empty;
        }
    }
}
