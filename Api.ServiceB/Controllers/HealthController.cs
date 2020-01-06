using Microsoft.AspNetCore.Mvc;

namespace Api.ServiceB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check() => Ok("ok");
    }
}