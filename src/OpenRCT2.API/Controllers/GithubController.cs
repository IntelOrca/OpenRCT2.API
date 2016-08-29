#if _ENABLE_GITHUB_PUSH_

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.Controllers
{
    public class GithubController : Controller
    {
        // TODO move these to environment or configuration variables
        private const string UpdateScriptPath = "/home/openrct2/update.sh";
        private const string GitHubHookSecret = "5ec87b23d89d984932459877fb1a7df1";
        private const string DeployGitRef = "refs/heads/master";

        public class JGitHubRepositoryEvent
        {
            public string @ref { get; set; }
        }

        [Route("github/push")]
        [HttpPost]
        public async Task<object> OnPush(
            [FromHeader(Name = "X-GitHub-Event")] string eventType,
            [FromHeader(Name = "X-Hub-Signature")] string githubHash)
        {
            // Only listen for push events
            if (eventType == "push")
            {
                byte[] payload = await Request.Body.ReadToBytesAsync();
                if (!VerifySignature(payload, githubHash))
                {
                    return new StatusCodeResult(401);
                }

                string payloadAsString = Encoding.UTF8.GetString(payload);
                var eventInfo = JsonConvert.DeserializeObject<JGitHubRepositoryEvent>(payloadAsString);

                // Check out the event details
                if (eventInfo.@ref == DeployGitRef)
                {
                    // Execute the update script, delay it so we don't immediately shut down the web app
                    Task delayedTask = Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        Process.Start("bash", UpdateScriptPath);
                    });
                }
            }
            
            return new
            {
                status = "success"
            };
        }

        private bool VerifySignature(byte[] payload, string expectedSignature)
        {
            string ourSignature = "sha1=" + ComputeSignature(payload, GitHubHookSecret);
            return ourSignature == expectedSignature;
        }

        private static string ComputeSignature(byte[] messageBytes, string secret)
        {
            byte[] keyBytes = System.Text.Encoding.ASCII.GetBytes(secret ?? "");
            using (var hmacsha1 = new HMACSHA1(keyBytes))
            {
                byte[] hash = hmacsha1.ComputeHash(messageBytes);
                return hash.ToHexString();
            }
        }
    }
}

#endif
