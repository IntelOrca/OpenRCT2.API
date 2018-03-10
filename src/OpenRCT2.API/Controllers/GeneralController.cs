using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;

namespace OpenRCT2.API.Controllers
{
    public class GeneralController : Controller
    {
        private readonly ApiConfig _apiConifg;

        public GeneralController(IOptions<ApiConfig> apiConifg)
        {
            _apiConifg = apiConifg.Value;
        }

        [HttpGet("/")]
        public object ApiMap()
        {
            var bp = _apiConifg.BaseUrl ?? $"{Request.Scheme}://{Request.Host}";
            return new
            {
                BuildsUrl = $"{bp}/build",
                LocalisationUrl = $"{bp}/localisation",
                NewsUrl = $"{bp}/news",
                ServersUrl = $"{bp}/servers",
                UsersUrl = $"{bp}/user",
            };
        }
    }
}
