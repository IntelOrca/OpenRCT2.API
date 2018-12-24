using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace OpenRCT2.API.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string OpenRCT2ClientName = "OpenRCT2";

        public static Version GetOpenRCT2ClientVersion(this HttpRequest request)
        {
            bool noUserAgents = true;
            foreach (UserAgent userAgent in request.GetUserAgents())
            {
                noUserAgents = false;
                if (userAgent.Name == OpenRCT2ClientName)
                {
                    Version version;
                    if (Version.TryParse(userAgent.Version, out version))
                    {
                        return version;
                    }
                    break;
                }
            }

            if (noUserAgents)
            {
                return new Version(0, 0, 4);
            }

            return null;
        }

        private static IEnumerable<UserAgent> GetUserAgents(this HttpRequest request)
        {
            IHeaderDictionary headers = request.Headers;
            StringValues userAgents = headers[HeaderNames.UserAgent];
            string allUserAgents = String.Join(" ", userAgents.ToArray());
            string[] userAgentParts = allUserAgents.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < userAgentParts.Length; i++)
            {
                string name = null;
                string version = null;
                string comment = null;

                string userAgent = userAgentParts[i];
                int slashIndex = userAgent.IndexOf('/');
                if (slashIndex == -1)
                {
                    name = userAgent;
                }
                else
                {
                    name = userAgent.Substring(0, slashIndex);
                    version = userAgent.Substring(slashIndex + 1);
                }

                if (i < userAgentParts.Length - 1)
                {
                    string nextPart = userAgentParts[i + 1];
                    if (nextPart.StartsWith("("))
                    {
                        comment = nextPart;

                        // Skip the next part
                        i++;
                    }
                }

                yield return new UserAgent(name, version, comment);
            }
        }
    }

    internal struct UserAgent
    {
        public string Name { get; }
        public string Version { get; }
        public string Comment { get; }

        public UserAgent(string name, string version, string comment)
        {
            Name = name;
            Version = version;
            Comment = comment;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            if (Version != null)
            {
                sb.Append('/');
                sb.Append(Version);
            }
            if (Comment != null)
            {
                sb.Append(' ');
                sb.Append('(');
                sb.Append(Comment);
                sb.Append(')');
            }
            return sb.ToString();
        }
    }
}
