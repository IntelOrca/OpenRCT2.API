using System.Net;
using System.Threading.Tasks;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.AppVeyor
{
    public class AppVeyorService : IAppVeyorService
    {
        private const string ApiUrl = "https://ci.appveyor.com/api/";

        public class JAppVeyorResponse
        {
            public JProject project { get; set; }
            public JBuild build { get; set; }
        }

        public Task<JBuild> GetLastBuild(string account, string project)
        {
            return GetLastBuild(account, project, null);
        }

        public async Task<JBuild> GetLastBuild(string account, string project, string branch)
        {
            string url = $"{ApiUrl}/projects/{account}/{project}";
            if (branch != null)
            {
                url += "/branch/{branch}";
            }

            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.ContentType = MimeTypes.ApplicationJson;

            JAppVeyorResponse response = await request.GetJsonResponse<JAppVeyorResponse>();
            return response.build;
        }
    }
}
