using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Asp.Net.Core.Web.Api.Template.With.Swagger.Model.Response;

namespace Asp.Net.Core.Web.Api.Template.With.Swagger.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwaggerTutorialController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SwaggerTutorialController> _logger;

        public SwaggerTutorialController(ILogger<SwaggerTutorialController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// PRE Etiketini ve returns XML düğümüne dair.
        /// </summary>
        /// 
        /// <remarks>
        /// ### PRE Etiketini Kullanmak ###
        /// HTML'in PRE etikeni Swagger UI görüntüler.
        /// - Pre etiketi XML yorum düğümlerinden değildir!
        /// - Ancak Pre etiketi Swagger UI tarafından görüntülenirken kullanılır
        /// 
        /// Böylece yeni satır ve boşluk karakterleri Swagger UI içinde düzgün görüntülenir.
        /// 
        /// <pre>
        /// return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
        ///    Date = DateTime.Now.AddDays(index),
        ///    TemperatureC = rng.Next(-20, 55),
        ///    Summary = Summaries[rng.Next(Summaries.Length)]
        /// }).ToArray();
        /// </pre>
        /// 
        /// ### returns Swagger UI Tarafından İşlenmez ###
        /// Swagger returns etiketini görüntülemez ! :(
        /// 
        /// 
        /// </remarks>
        /// <returns>Swagger returns etiketini görüntülemez ! :(</returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }


        /// <summary>
        /// Markdown kullanımı ve kod örneklerinin gösterimi.
        /// </summary>
        /// 
        /// <remarks>
        /// _Swagger maalesef [Tüm XML Düğümlerini](https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2013/y3ww3c7e(v=vs.120)) desteklemiyor!_ 
        /// Bu yüzden markdown kullanarak XML yorumlarının dışında ama SwaggerUI için istenilen çıktıyı elde edebiliriz. 
        /// 
        /// Swagger code etiketindeki bilgiyi görüntülerken renkli gösteriyor 
        /// ancak line-break karakterlerini işleyemediği için tek satırda gösteriyor.
        /// 
        /// **Bu yüzden satır sonuna \ ekliyoruz ki yeni satıra geçsin.**
        /// 
        /// <code> \
        /// return Enumerable.Range(1, 5).Select(index => new WeatherForecast { \
        ///     Date = DateTime.Now.AddDays(index), \
        ///     TemperatureC = rng.Next(-20, 55), \
        ///     Summary = Summaries[rng.Next(Summaries.Length)] \
        /// }).ToArray(); \
        /// </code>
        /// 
        /// ### Code Etiketi ###
        /// Code etiketinde yeni satıra geçmiyor, satır içi kullanılıyordu. Kod metnini işaretlemek <code>int? code=12;</code> kullanabiliriz.
        /// 
        /// ### Markdown ###
        /// Ayrıca Markdown kullanarak satır içi kod metnini işaretlemek için kesme işareti (apostrof) kullanabiliriz; `int markdown=12;` örneğindeki gibi.
        /// </remarks>
        /// <param name="dayCount">Tam sayı tipinde parametre.</param>
        /// <returns>Sandcastle tarafından kod belgesi oluşturulurken değerlendirilir ama Swagger bu etiketi işlemez!</returns>
        [HttpGet("{dayCount}")]
        //[ProducesResponseType(200)]
        public ListResponse<WeatherForecast> Get(int dayCount)
        {
            var rng = new Random();
            var result = new ListResponse<WeatherForecast>();
            result.Model =
                Enumerable.Range(1, dayCount).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
            .ToArray();
            return result;
        }

        /// <summary>
        /// Bir action için yazılabilecek XML yorum satırları ve Swagger UI çıktısına dair.
        /// </summary>
        /// 
        /// <param name="predefinedId">Kayıt edilecek bilginin önceden belirlenmiş ID değeri.</param>
        /// <param name="userId">Bilgiyi kaydedecek kullanıcının ID değeri. Mecburi olmasın diye int? tipinde.</param>
        /// <param name="wcast"><see cref="WeatherForecast"/> tipinde değeri metoda parametre olarak alır </param>
        /// 
        /// <response code="200">Başarıyla tamamlanan işin açıklaması. Sorun yaşanmadı.</response>
        /// <response code="201">Nesne tanımı yapıldı.</response>
        /// <response code="101">Yazılmayan response kodunun açıklaması varsayılan metin olacak.</response>
        /// <response code="401">Yetkisiz erişim.</response>
        /// <response code="500">Sunucu hatası dönüş tipine bakmaz doğrudan string tipinde sonuç döner.</response>
        /// 
        /// <returns>Dönen değere dair bilgi. SWAGGER Buradaki bilgiyi görüntülemiyor maalesef. Neden mi?</returns>
        [HttpPost("{predefinedId}")]
        /* Formun byte türünden alabileceği maksimum boyutu */
        [RequestSizeLimit(100_000_000)] 

        [Consumes("application/json", "application/xml", "application/x-www-form-urlencoded")]
        [Produces("application/json", "application/xml")]

        /* Type bilgisini yazmazsak Swagger metodun dönüş tipinden üretir. */
        [ProducesResponseType(200)]

        /* Type bilgisini yazmazsak Swagger metodun dönüş tipinden üretir. */
        [ProducesResponseType(StatusCodes.Status201Created)]

        /* 301 Hata kodu için sadece "Redirect" yazar. */
        [ProducesResponseType(301)]

        /* 401 Hata kodu için Type bilgisini yazmazsak dönüş tipi otomatik üretilir:
         * {
              "type": "string",
              "title": "string",
              "status": 0,
              "detail": "string",
              "instance": "string",
              "extensions": {
                "additionalProp1": {},
                "additionalProp2": {},
                "additionalProp3": {}
              }
         * }
         */
        [ProducesResponseType(401)]

        /* 501 Hata kodu için Type bilgisini yazmazsak dönüş tipi üretilmez! Sadece "Server Error" yazar! */        
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]

        /* 500 Hata kodu için dönüş tipinin string olacağını söylüyoruz! */
        /// <response code="500">Sunucu hatası dönüş tipine bakmaz doğrudan string tipinde sonuç döner.</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public Response Post(int predefinedId, int? oylesine, [FromBody] WeatherForecast wcast)
        {
            return new Response() { };
        }

    }
}
