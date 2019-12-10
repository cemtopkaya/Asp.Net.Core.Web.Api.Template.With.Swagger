using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers {

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProducesResponseFormatController : ControllerBase {
        [HttpGet]
        [Produces("application/xml")]
        public IActionResult Get() {
            /**
             * Anonymous tiplerin XML dönüşümleri gerçekleştirilemez. HTTP 406 hatası döner.
             *    return Ok(new { a = "aaaa" });
             * Bu yüzden tanımlı tiplerin nesnelerini cevap olarak dönmeliyiz
             *    return Ok(new ConcreteClass());
             */
            return Ok(new Model.appsettings.TokenSettings());
        }
    }
}