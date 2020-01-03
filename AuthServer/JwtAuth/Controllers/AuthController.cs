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
    [Route("api/[controller]")]
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
        [HttpPost]
        public IActionResult Login([FromBody]LoginModel loginModel)
        {


            _logger.LogInformation($"{loginModel.UserName} login！");
            if (loginModel.Password == "111111")
            {
                var ip = HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                var claims = new Claim[] {
                        new Claim(ClaimTypes.Name, "xp"),
                        new Claim(ClaimTypes.Role, "admin")
                    };
                switch (loginModel.UserName)
                {
                    case "xp"://过期时间为500000
                        var token1 = _tokenBuilder.BuildJwtToken(claims, ip, DateTime.UtcNow, DateTime.Now.AddSeconds(500000));
                        _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
                        return new JsonResult(new { Result = true, Data = token1 });
                    case "ggg"://过期时间为30
                        var token2 = _tokenBuilder.BuildJwtToken(claims, DateTime.Now.AddSeconds(30));
                        _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
                        return new JsonResult(new { Result = true, Data = token2 });
                    default:
                        return null;
                }
            }
            else
            {
                _logger.LogInformation($"{loginModel.UserName} login failed");
                return new JsonResult(new
                {
                    Result = false,
                    Message = "Authentication Failure"
                });
            }
        }
    }
}