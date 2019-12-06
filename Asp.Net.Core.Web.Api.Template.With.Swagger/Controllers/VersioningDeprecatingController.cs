using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    //[ApiVersion("6.0")]
    //[Route("api/v{version:apiVersion}/versioning")]
    //[ApiController]
    //public class Versioning_Deprecating_Controller_v6 : ControllerBase
    //{
    //    [HttpGet]
    //    public IActionResult Get() => Ok("URL Path Segment - version 6.0");
    //}

    [ApiVersion("6.1")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v61 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 6.1");
    }

    [ApiVersion("6.2")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v62 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 6.2");
    }

    /*
     * Deprecated özelliğiyle 6.0 versiyonunun kaldırıldığını yerine 
     * 6.1, 6.2, 7.0'ın kullanılabileceğini bildiriyoruz.
     */
    [ApiVersion("6.0", Deprecated = true)]
    [ApiVersion("7.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_Deprecating_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion version)
        {
            return Ok(new
            {
                Method = "URL Path Segment",
                Requested_Version = version,
                Description = "7.0 için evet ama 6.0'a el salla!"
            });
        }
    }
}