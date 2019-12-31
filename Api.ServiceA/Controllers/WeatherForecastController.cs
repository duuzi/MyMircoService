using Api.ServiceA.IntegrationEvents.Events;
using Cp.EventBus.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.ServiceA.Controllers
{
    [ApiController]
    [Route("apiservice/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IEventBus _eventBus;

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEventBus eventBus)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            var @event = new AChangedEvent()
            {
                NewPrice = 100,
                OldPrice = 99,
                ProductId = 1
            };
            _logger.LogInformation("A发布消息");
            _eventBus.Publish(@event);
            return "A";
        }

        [HttpGet("/health")]
        public IActionResult Heathle()
        {
            return Ok();
        }
    }
}
