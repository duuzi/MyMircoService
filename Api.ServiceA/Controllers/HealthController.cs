using Microsoft.AspNetCore.Mvc;

namespace Api.ServiceA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check() => Ok("ok");
    }
}