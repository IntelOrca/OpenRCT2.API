using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;

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
    }
}
