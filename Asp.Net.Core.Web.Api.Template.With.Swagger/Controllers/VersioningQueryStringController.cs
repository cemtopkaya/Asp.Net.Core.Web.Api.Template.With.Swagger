using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers {
    [ApiVersion("1.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v1 : ControllerBase {
        [HttpGet]
        public IActionResult Get() => Ok("QueryString - version 1.0");
    }

    [ApiVersion("2.0")]
    [ApiVersion("3.0")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v3 : ControllerBase {
        [HttpGet("{id}")]
        public IActionResult Get() => Ok("QueryString - version 2.0 ~ 3.0");

        /* Aynı versiyon numarasını (buradaki örnekte 3.0 versiyonudur) iki denetleyici içinde kullanırsak
         *  - hem Versioning_QueryString_Controller_v3 sınıfta 
         *  - hem de Versioning_QueryString_Controller_v4 sınıfta 
         * Swagger aynı rota bilgisine sahip uç noktaları hangi sınıfta göstereceğini bilemeyeceği için 
         * uç noktayı test etmek istediğinde hata alacağı için görüntülemede de hata üretir.
         * 
         * Bir denetleyici üstünde bir ApiVersion niteliği o denetleyicinin versiyon numarasını gösterirken
         * başka ApıVersion nitelikleri Deprecated olanları işaretlemek içindir.
         * 
         * Aynı versiyon numarası farklı denetleyicilerde kullanılırsa, eğer uç nokta rotaları çatışmıyorsa 
         * sorunsuz olarak Swagger çalışır. Bu durumda örneğin 1.0 versiyonunu 2.0 versiyonunda da geçerli kılacaksak
         * o zaman neden 2.0 versiyonunu yazalım ki :)
         * 
         * Eğer 2.0 versiyonu, 1.0 versiyonunun üstüne gelecek ve yeni metotlar içerecekse "kalıtım" en iyi çözüm olacak.
         * Böylece 2.0 versiyonunda 1.0 versiyonundaki metotları tekrar yazmak zorunda kalmayız. 
         * Eğer 2.0 da XXX metodu daha farklı çalışacaksa 1.0 sürümündeki halini virtual diye işaretler, 2.0 da override edebiliriz.
         * Eğer 1.0 versiyonunda olan bir metodu 2.0 da kullanılmaz olarak işaretlemek istersek 
         * en basit haliyle override ettiğimiz metodun içinde NotFound() gönderebiliriz.
         */
        //[HttpGet]
        //public IActionResult Get() => Ok("QueryString - version 2.0 ~ 3.0");
    }

    [ApiVersion("3.0")]
    [ApiVersion("4.0-Beta")]
    [ApiVersion("4.0")]
    [ApiVersion("4.1")]
    [ApiVersion("4.5")]
    [Route("api/versioning")]
    [ApiController]
    public class Versioning_QueryString_Controller_v4 : ControllerBase {
        [HttpGet]
        public IActionResult Get(ApiVersion v) {
            return Ok(new {
                Method = "QueryString",
                Requested_Version = v,
                Description = "4.0-Alpha< version <=4.5"
            });
        }
    }
}