using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SieveFramework.AspNetCore.Models;
using SieveFramework.Providers;

namespace WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ISieveProvider _sieve;

        public WeatherForecastController(ISieveProvider sieve)
        {
            _sieve = sieve;
        }

        [HttpGet]
        public ActionResult<WeatherForecast> Get(Sieve<WeatherForecast> model)
        {
            var rng = new Random();
            var query = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).AsQueryable();

            return Ok(new
            {
                origin = query.ToArray(),
                filtered = _sieve.Apply(query, model).ToArray()
            });
        }
    }
}
