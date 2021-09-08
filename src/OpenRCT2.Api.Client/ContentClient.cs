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

        public Task<UploadContentResponse> Upload(UploadContentRequest request)
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent(request.Owner ?? ""), "owner" },
                { new StringContent(request.Name ?? ""), "name" },
                { new StringContent(request.Description ?? ""), "description" },
                { new StringContent(request.Visibility.ToString()), "visibility" },
                { new StreamContent(request.File), "file", request.FileName },
                { new StreamContent(request.Image), "image", request.ImageFileName }
            };
            return _client.PostAsync<UploadContentResponse, MultipartFormDataContent, UploadContentResponse>("content/upload", form);
        }
    }
}
