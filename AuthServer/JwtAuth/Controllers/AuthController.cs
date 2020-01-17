using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocelot.JwtAuthorize;

namespace JwtAuth.Controllers
{
    [Route("auth/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly ILogger<AuthController> _logger;
        readonly ITokenBuilder _tokenBuilder;
        public AuthController(ITokenBuilder tokenBuilder, ILogger<AuthController> logger)
        {
            _logger = logger;
            _tokenBuilder = tokenBuilder;

        }
        [Route("Check")]
        [HttpGet]
        public IActionResult Check() => Ok("ok");
        [HttpPost]
        public IActionResult Login([FromForm]LoginModel loginModel)
        {
            _logger.LogInformation($"{loginModel.UserName} login！");
            if (loginModel.Password == "admin")
            {
                var ip = HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                var claims = new Claim[] {
                        new Claim(ClaimTypes.Name, "xp"),
                        new Claim(ClaimTypes.Role, "admin")
                    };
                switch (loginModel.UserName)
                {
                    case "admin"://过期时间为500000
                        var token1 = _tokenBuilder.BuildJwtToken(claims, ip, DateTime.UtcNow, DateTime.Now.AddSeconds(500000));
                        _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
                        return new JsonResult(new { Code = 200, Data = token1.TokenValue });
                    case "ggg"://过期时间为30
                        var token2 = _tokenBuilder.BuildJwtToken(claims, DateTime.Now.AddSeconds(30));
                        _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
                        return new JsonResult(new { Code = 200, Data = token2 });
                    default:
                        return null;
                }
            }
            else
            {
                _logger.LogInformation($"{loginModel.UserName} login failed");
                return new JsonResult(new
                {
                    Code = 500,
                    Message = "Authentication Failure"
                });
            }
        }
    }
}