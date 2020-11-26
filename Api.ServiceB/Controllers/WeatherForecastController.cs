using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.ServiceB.Controllers
{
    [Authorize("permission")]
    [ApiController]
    [Route("apiservice/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("ServiceB执行");
            return "B";
        }

        [Route("/getNeedAuth")]
        [HttpGet]
        public string GetNeedAuth()
        {
            _logger.LogInformation("GetNeedAuth");
            return "GetNeedAuth";
        }
    }
}
