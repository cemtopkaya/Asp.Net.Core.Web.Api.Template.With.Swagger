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
    public class ParameterBindingTutorialController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ParameterBindingTutorialController> _logger;

        public ParameterBindingTutorialController(ILogger<ParameterBindingTutorialController> logger)
        {
            _logger = logger;
        }

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

        [HttpGet("{dayCount}")]
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

        //[HttpPost("body/{id}")]
        //[Consumes("application/x-www-form-urlencoded")]
        ///* Formun byte türünden alabileceği maksimum boyutu */
        //[RequestSizeLimit(100_000_000)]
        //public SingleResponse<WeatherForecast> Post_FromBody(int id, [FromBody] WeatherForecast wcast)
        //{
        //    return new SingleResponse<WeatherForecast>()
        //    {
        //        Model = wcast
        //    };
        //}

        [HttpPost("form/{id}")]
        [Consumes("application/json", "application/x-www-form-urlencoded")]
        public WeatherForecast Post_FromForm(int id, [FromForm] WeatherForecast wcast) => wcast;


        [HttpPost("body/{id}")]
        [Consumes("application/json")]
        public WeatherForecast Post_FromBody(int id, [FromBody] WeatherForecast wcast) => wcast;

    }
}
