using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenRCT2.API.Abstractions;
using OpenRCT2.DB.Abstractions;
using OpenRCT2.DB.Models;

namespace OpenRCT2.API.Controllers
{
    public class ObjectController : Controller
    {
        private readonly IRctObjectRepository _objectRepository;

        public ObjectController(IRctObjectRepository objectRepository)
        {
            _objectRepository = objectRepository;
        }

        [HttpGet("objects")]
        public async Task<object> GetAll()
        {
            var objects = await _objectRepository.GetAllAsync();
            return new
            {
                status = JStatus.OK,
                objects = objects
            };
        }

        [HttpPost("objects/download")]
        public async Task<object> Download(
            [FromBody] JDownloadObjectsRequest request)
        {
            var foundObjects = new List<RctObject>();
            if (request?.names != null)
            {
                foreach (var name in request.names)
                {
                    var objects = await _objectRepository.ByNameAsync(name);
                    foundObjects.AddRange(objects);
                }
            }
            if (request?.headers != null)
            {
                foreach (var header in request.headers)
                {
                    var objects = await _objectRepository.ByHeaderAsync(header);
                    foundObjects.AddRange(objects);
                }
            }
            return new
            {
                status = JStatus.OK,
                objects = foundObjects.Select(
                    obj => new
                    {
                        name = obj.Name,
                        header = obj.Header,
                        downloadAddress = obj.DownloadAddress
                    }).ToArray()
            };
        }

        public class JDownloadObjectsRequest
        {
            public string[] names { get; set; }
            public string[] headers { get; set; }
        }
    }
}
