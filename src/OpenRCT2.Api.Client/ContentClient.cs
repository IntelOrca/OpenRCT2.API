using System.Net.Http;
using System.Threading.Tasks;
using OpenRCT2.Api.Client.Models;

namespace OpenRCT2.Api.Client
{
    public class ContentClient
    {
        private readonly OpenRCT2ApiClient _client;

        internal ContentClient(OpenRCT2ApiClient client)
        {
            _client = client;
        }

        public Task<ContentModel[]> Get(string owner)
        {
            return _client.GetAsync<ContentModel[]>(_client.UrlEncode("content?owner={0}", owner));
        }

        public Task<ContentModel> Get(string owner, string name)
        {
            return _client.GetAsync<ContentModel>(_client.UrlEncode("content/{0}/{1}", owner, name));
        }

        public Task<ContentModel[]> GetRecent() => _client.GetAsync<ContentModel[]>("content/recent");
        public Task<ContentModel[]> GetPopular () => _client.GetAsync<ContentModel[]>("content/popular");

        public Task<UploadContentResponse> Upload(UploadContentRequest request)
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Owner ?? ""), "owner" },
                { new StringContent(request.Name ?? ""), "name" },
                { new StringContent(request.Title ?? ""), "title" },
                { new StringContent(request.Description ?? ""), "description" },
                { new StringContent(request.Visibility.ToString()), "visibility" },
                { new StreamContent(request.File), "file", request.FileName },
                { new StreamContent(request.Image), "image", request.ImageFileName }
            };
            return _client.PostAsync<UploadContentResponse, MultipartFormDataContent, UploadContentResponse>("content/upload", form);
        }

        public Task<UploadContentResponse> Update(string owner, string name, UploadContentRequest request)
        {
            var url = _client.UrlEncode("content/{0}/{1}", owner, name);
            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Owner ?? ""), "owner" },
                { new StringContent(request.Name ?? ""), "name" },
                { new StringContent(request.Title ?? ""), "title" },
                { new StringContent(request.Description ?? ""), "description" },
                { new StringContent(request.Visibility.ToString()), "visibility" },
            };
            if (request.File != null)
            {
                form.Add(new StreamContent(request.File), "file", request.FileName);
            }
            if (request.Image != null)
            {
                form.Add(new StreamContent(request.Image), "image", request.ImageFileName);
            }
            return _client.PutAsync<UploadContentResponse, MultipartFormDataContent, UploadContentResponse>(url, form);
        }

        public Task<VerifyContentNameResponse> VerifyName(string owner, string name)
        {
            var url = _client.UrlEncode("content/verifyName?owner={0}&name={1}", owner, name);
            return _client.GetAsync<VerifyContentNameResponse>(url);
        }

        public Task SetLike(string owner, string name, bool value)
        {
            var url = _client.UrlEncode("content/{0}/{1}/like", owner, name);
            return value ?
                _client.PostAsync<object>(url) :
                _client.DeleteAsync<object>(url);
        }
    }
}
