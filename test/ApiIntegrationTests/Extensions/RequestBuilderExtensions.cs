using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace ApiIntegrationTests.Extensions
{
    internal static class RequestBuilderExtensions
    {
        public static RequestBuilder UseJsonBody<TModel>(this RequestBuilder builder, TModel body)
        {
            string bodyAsString = JsonConvert.SerializeObject(body);
            builder.And(req =>
            {
                req.Content = new StringContent(bodyAsString, Encoding.UTF8, "application/json");
            });
            return builder;
        }

        public static async Task<TModel> SendAsJsonAsync<TModel>(this RequestBuilder builder, HttpMethod method)
        {
            HttpResponseMessage response = await builder.SendAsync(method.Method);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<TModel>(responseString);
            return responseJson;
        }

        public static Task<TModel> GetAsJsonAsync<TModel>(this RequestBuilder builder)
        {
            return SendAsJsonAsync<TModel>(builder, HttpMethod.Get);
        }

        public static Task<TModel> PostAsJsonAsync<TModel>(this RequestBuilder builder)
        {
            return SendAsJsonAsync<TModel>(builder, HttpMethod.Post);
        }
    }
}
