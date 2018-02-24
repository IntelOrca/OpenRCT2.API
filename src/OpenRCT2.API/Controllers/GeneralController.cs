using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
                UsersUrl = $"{bp}/user",
                LocalisationUrl = $"{bp}/localisation",
                ServersUrl = $"{bp}/servers"
            };
        }
    }
}
