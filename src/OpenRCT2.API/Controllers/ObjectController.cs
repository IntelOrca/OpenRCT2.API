using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            [FromRoute] string name)
        {
            var normalisedName = name.ToUpperInvariant();
            var result = await rctObjectRepo.GetLegacyFromNameAsync(normalisedName);
            if (result != null)
            {
                var nedesignsId = result.Id;
                var nedesignsUrl = $"https://www.nedesigns.com/rct2-object/{nedesignsId}/{name}/download/";
                return new
                {
                    name = normalisedName,
                    download = nedesignsUrl
                };
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
