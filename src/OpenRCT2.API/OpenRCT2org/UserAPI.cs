using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.OpenRCT2org
{
    public class UserApi : IUserApi
    {
        private const string ApiUrl = "https://openrct2.org/forums/jsonapi.php";

        private readonly UserApiOptions _options;

        private string AppToken => _options.ApplicationToken;

        public UserApi(IOptions<UserApiOptions> options)
        {
            _options = options.Value;
        }

        public async Task<JUser> GetUser(int id)
        {
            HttpWebRequest request = WebRequest.CreateHttp(ApiUrl);
            request.ContentType = MimeTypes.ApplicationJson;
            request.Method = "POST";

            await request.WritePayload(new
            {
                key = AppToken,
                command = "getUser",
                userId = id
            });

            string responseJson = await GetPayload(request);
            var jResponse = JsonConvert.DeserializeObject<JResponse>(responseJson);
            if (jResponse.error != 0)
            {
                throw new OpenRCT2orgException(jResponse);
            }

            var user = JsonConvert.DeserializeObject<JUser>(responseJson);
            return user;
        }

        public async Task<JUser> AuthenticateUser(string userName, string password)
        {
            HttpWebRequest request = WebRequest.CreateHttp(ApiUrl);
            request.ContentType = MimeTypes.ApplicationJson;
            request.Method = "POST";

            await request.WritePayload(new
            {
                key = AppToken,
                command = "authenticate",
                name = userName,
                password = password
            });

            string responseJson = await GetPayload(request);
            var jResponse = JsonConvert.DeserializeObject<JResponse>(responseJson);
            if (jResponse.error != 0)
            {
                throw new OpenRCT2orgException(jResponse);
            }

            var user = JsonConvert.DeserializeObject<JUser>(responseJson);
            return user;
        }

        private static async Task<string> GetPayload(HttpWebRequest request)
        {
            using (HttpWebResponse response = await request.GetHttpResponseAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new OpenRCT2orgException(ErrorCodes.InternalError, "Unsuccessful response from server.");
                }

                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}
