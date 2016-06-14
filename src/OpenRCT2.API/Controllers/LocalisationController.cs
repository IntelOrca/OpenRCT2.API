using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Extensions;

namespace OpenRCT2.API.Controllers
{
    public class LocalisationController : Controller
    {
        private static ConcurrentDictionary<int, string> CachedProgressImages = new ConcurrentDictionary<int, string>();

        [Route("localisation/status/badges/{languageId}")]
        public async Task<object> GetBadgeStatus(
            [FromServices] ILocalisationService localisationService,
            [FromRoute] string languageId)
        {
            int progress = await localisationService.GetLanguageProgressAsync(languageId);
            string progressSvg = await GetProgressImageAsync(progress);
            if (progressSvg == null)
            {
                return new StatusCodeResult(404);
            }

            return Content(progressSvg, MimeTypes.ImageSvgXml);
        }

        private static async Task<string> GetProgressImageAsync(int progress)
        {
            string svg;
            if (CachedProgressImages.TryGetValue(progress, out svg))
            {
                return svg;
            }

            HttpWebRequest request = WebRequest.CreateHttp($"http://progressed.io/bar/{progress}");
            using (HttpWebResponse response = await request.GetHttpResponseAsync())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var responseStream = response.GetResponseStream();
                var gzip = new GZipStream(responseStream, CompressionMode.Decompress);
                var sr = new StreamReader(gzip);
                svg = await sr.ReadToEndAsync();

                CachedProgressImages.TryAdd(progress, svg);
                return svg;
            }
        }
    }
}
