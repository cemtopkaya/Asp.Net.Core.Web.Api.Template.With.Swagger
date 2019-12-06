using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v1 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("URL Path Segment - version 1.0");
    }

    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [ApiVersion("3.1")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion version) => Ok(version + " URL Path Segment - version 2.0 ~ 3.1");
    }


    [ApiVersion("4.0-Alpha")]
    [ApiVersion("4.1")]
    //[ApiVersion("4.1.1")] // Çalıştırılamaz çünkü 3. grup string tipinde Status olmalı!
    [ApiVersion("4.2")]
    [ApiVersion("4.3-Alpha")]
    [ApiVersion("4.3-Beta")]
    [ApiVersion("4.3-RC1")]
    [ApiVersion("4.3-RC2")]
    [ApiVersion("4.3")]
    [ApiVersion("4.4")]
    [ApiVersion("4.5")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v4 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion v)
        {
            return Ok(new
            {
                Method = "URL Path Segment",
                Requested_Version = v,
                Description = "4.0-Alpha< version <=4.5"
            });
        }
    }
}