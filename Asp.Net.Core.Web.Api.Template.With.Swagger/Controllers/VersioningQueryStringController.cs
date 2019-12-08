using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v1 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("QueryString - version 1.0");
    }

    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v2 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("QueryString - version 2.0 ~ 3.0");
    }

    [ApiVersion("3.0")]
    [ApiVersion("4.0-Beta")]
    [ApiVersion("4.0")]
    [ApiVersion("4.1")]
    [ApiVersion("4.5")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v4 : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ApiVersion v)
        {
            return Ok(new
            {
                Method = "QueryString",
                Requested_Version = v,
                Description = "4.0-Alpha< version <=4.5"
            });
        }
    }
}