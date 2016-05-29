using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OpenRCT2.API.Controllers
{
    public class GithubController : Controller
    {
        public class JGitHubPushRequest
        {
            public string action { get; set; }
        }

        [Route("github/push")]
        [HttpPost]
        public void OnPush(JGitHubPushRequest request)
        {
            Process.Start("bash", "/home/openrct2/update.sh");
        }
    }
}
