using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v1 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get1() => Ok("URL Path Segment - version 1.0");
    }

    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get2(ApiVersion version) => Ok(version + " URL Path Segment - version 2.0");
    }

    [ApiVersion("2.0", Deprecated = true)]
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v3 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get2(ApiVersion version)
        {
            return Ok(version + " URL Path Segment - version 3.0");
        }
    }


    /* Çalıştırılamaz çünkü 3. grup string tipinde Status olmalı! */
    //[ApiVersion("4.0.1")] 
    /* Bir denetleyici sınıf aktif bir versiyonlam niteliğine ve 
     * isteğe bağlı olarak Deprecated olan versiyonla niteliğine sahip olmalı!
     */
    // [ApiVersion("4.0-Alpha")]
    // [ApiVersion("4.1")]
    // [ApiVersion("4.2")]
    // [ApiVersion("4.3-Alpha")]
    // [ApiVersion("4.3-Beta")]
    // [ApiVersion("4.3-RC1")]
    // [ApiVersion("4.3-RC2")]
    // [ApiVersion("4.3")]
    // [ApiVersion("4.4")]
    [ApiVersion("4.5")]
    [Route("api/v{version:apiVersion}/versioning")]
    [ApiController]
    public class Versioning_UrlPathSegment_Controller_v4 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get3(ApiVersion v)
        {
            return Ok(new
            {
                Method = "URL Path Segment",
                Requested_Version = v,
                Description = "version 4.5"
            });
        }
    }
}