using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace OpenRCT2.Api.Client
{
    public sealed class OpenRCT2ApiClient : IDisposable
    {
        public const string DEFAULT_URI = "https://api.openrct2.io";
        private const string MIME_JSON = "application/json";
        private const string CLIENT_NAME = "OpenRCT2.Api.Client";
        private const string CLIENT_VERSION = "1.0";

        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private HttpClient _httpClient;

        public Uri BaseAddress { get; }
        public string AuthorizationToken { get; }
        public string ApiKey { get; }

        public AuthClient Auth { get; }
        public UserClient User { get; }

        public OpenRCT2ApiClient(string authorizationToken = null, string apiKey = null) : this(new Uri(DEFAULT_URI), authorizationToken, apiKey)
        {
        }

        public OpenRCT2ApiClient(Uri baseAddress, string authorizationToken = null, string apiKey = null)
        {
            BaseAddress = baseAddress;
            AuthorizationToken = authorizationToken;
            ApiKey = apiKey;
            Auth = new AuthClient(this);
            User = new UserClient(this);
        }

        public void Dispose()
        {
        }

        private HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MIME_JSON));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(CLIENT_NAME, CLIENT_VERSION));
                if (AuthorizationToken != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthorizationToken);
                }
                if (ApiKey != null)
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
                }
                _httpClient = client;
            }
            return _httpClient;
        }

        internal Task<T> GetAsync<T>(string url) => GetAsync<T, object>(url, null);

        internal async Task<T> GetAsync<T, TParameters>(string url, TParameters parameters = null) where TParameters : class
        {
            var client = GetHttpClient();

            var fullUri = new Uri(BaseAddress, url);
            if (parameters != null)
            {
                var queryArgs = HttpUtility.ParseQueryString(string.Empty);
                foreach (var propertyInfo in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var value = propertyInfo.GetValue(parameters);
                    if (value != null)
                    {
                        queryArgs.Add(ToCamelCase(propertyInfo.Name), value.ToString());
                    }
                }

                var ub = new UriBuilder(fullUri)
                {
                    Query = queryArgs.ToString()
                };
                fullUri = ub.Uri;
            }

            var response = await client.GetAsync(fullUri);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _serializerOptions);
            }
            else
            {
                throw new OpenRCT2ApiClientStatusCodeException(response.StatusCode);
            }
        }

        internal Task<T> PostAsync<T>(string url, object body = null) => PostAsync<T, object>(url, body);
        internal Task<T> PostAsync<T, TBody>(string url, TBody body = default) => PostAsync<T, TBody, object>(url, body);
        internal Task<T> PostAsync<T, TBody, TError>(string url, TBody body = default) => SendAsync<T, TBody, TError>(HttpMethod.Post, url, body);

        internal Task<T> PutAsync<T>(string url, object body = null) => PutAsync<T, object>(url, body);
        internal Task<T> PutAsync<T, TBody>(string url, TBody body = default) => SendAsync<T, TBody, object>(HttpMethod.Put, url, body);

        internal Task DeleteAsync(string url, object body = null) => DeleteAsync<object, object>(url, body);
        internal Task<T> DeleteAsync<T>(string url, object body = null) => DeleteAsync<T, object>(url, body);
        internal Task<T> DeleteAsync<T, TBody>(string url, TBody body = default) => SendAsync<T, TBody, object>(HttpMethod.Delete, url, body);

        private async Task<T> SendAsync<T, TBody, TError>(HttpMethod method, string url, TBody body = default)
        {
            var client = GetHttpClient();
            var fullUri = new Uri(BaseAddress, url);
            var content = new StringContent(JsonSerializer.Serialize(body, _serializerOptions), Encoding.UTF8, MIME_JSON);
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = fullUri,
                Content = content
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _serializerOptions);
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var resp = string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<TError>(json, _serializerOptions);
                throw new OpenRCT2ApiClientStatusCodeException(response.StatusCode, resp);
            }
        }

        private static string ToCamelCase(string s)
        {
            if (s != null && s.Length > 0)
            {
                if (char.IsUpper(s[0]))
                {
                    return char.ToLowerInvariant(s[0]) + s.Substring(1);
                }
            }
            return s;
        }
    }
}
