using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenRCT2.API.Extensions
{
    public static class HttpWebRequestExtensions
    {
        public static async Task WritePayload(this HttpWebRequest request, object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] payload = Encoding.UTF8.GetBytes(json);
            using (Stream contentStream = await request.GetRequestStreamAsync())
            {
                await contentStream.WriteAsync(payload, 0, payload.Length);
            }
        }

        public static async Task<HttpWebResponse> GetHttpResponseAsync(this HttpWebRequest request)
        {
            return await request.GetResponseAsync() as HttpWebResponse;
        }

        public static async Task<T> GetJsonResponse<T>(this HttpWebRequest request)
        {
            using (HttpWebResponse response = await request.GetHttpResponseAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new WebException("Server did not return 200 status code.");
                }

                var streamReader = new StreamReader(response.GetResponseStream());
                string json = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
    }
}
