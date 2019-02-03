using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Services;
using OpenRCT2.DB.Abstractions;

namespace OpenRCT2.API.Controllers
{
    public class ObjectController
    {
        [HttpGet("objects")]
        public object GetAll()
        {
            return new
            {
                status = JStatus.OK,
                objects = new[] { "GWTRITC", "GWTRJT1", "GWTRJT2" }
            };
        }

        [HttpGet("objects/legacy/{name}")]
        public async Task<object> GetAsync(
            [FromServices] IRctObjectRepository rctObjectRepo,
            [FromServices] NeDesignsService neDesignsService,
            [FromRoute] string name)
        {
            var normalisedName = name.ToUpperInvariant();
            var result = await rctObjectRepo.GetLegacyFromNameAsync(normalisedName);
            if (result != null)
            {
                var nedesignsUrl = neDesignsService.GetUrl(result.NeDesignId, result.Name);
                return new
                {
                    name = normalisedName,
                    download = nedesignsUrl
                };
            }
            else
            {
                if (neDesignsService.HasEnoughTimePassedToQuery())
                {
                    // Spin off a search in the background but return 404 for now
                    neDesignsService.SearchForNewObjectsAsync().Forget();
                }
                return new NotFoundResult();
            }
        }
    }
}
