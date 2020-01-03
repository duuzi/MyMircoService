Ocelot作为基于.net core的API方关，有一个功能是统一验证，它的作用是把没有访问权限的请求挡在API网关外面，而不是到达API网关事端的API时才去验证；之前我有一篇博文https://www.cnblogs.com/axzxs2001/p/8005084.html，作过说明，这篇博文说明了实现代码，今天我把这个实现作了整理，封装成一个Nuget包，供大家方便调用。

Web API的验证一般是用UserName和Password请求到Token，然后每次请求需要权限的API接口是把Token带到请求的Header中，作为凭据，API服端接收到请求后就要对客户端带的Token作验证，查看Token是否正确，是否过期，如果没有问题，再对该用户作权鉴，该用户是否有权限访问本API接口；这样看来，登录获取Tokent算一块，成功登录后，每次带Token请求又分两块：一块是验证，一块是鉴权，所以在Ocelot.JwtAuthorize中一共分三块。

项目的源码位于https://github.com/axzxs2001/Ocelot.JWTAuthorize

Nuget是https://www.nuget.org/packages/Ocelot.JwtAuthorize

使用也非常简单，首先有统一的配置文件（网关项目中，API项目中，验证项目中）

"JwtAuthorize": {
  "Secret": "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
  "Issuer": "gsw",
  "Audience": "everyone",
  "PolicyName": "permission",
  "DefaultScheme": "Bearer",
  "IsHttps": false,
  "Expiration": 50000
}

1、网关项目中在Startup的ConfigureService方法中注入services.AddOcelotJwtAuthorize()即可。

2、验证项目中在Startup的ConfigureService方法中注入services.AddTokenJwtAuthorize()，同时验证项目还有一个作用是分发Token，前提是用户有正确的用户名密码，所以要做一个登录的Colloer和Action来实现，注意登录时Claim中的信息是在API项目中验证权限的信息。
readonly ILogger<LoginController> _logger;
//ITokenBuilder是用来生成Token的
readonly ITokenBuilder _tokenBuilder;
public LoginController(ITokenBuilder tokenBuilder, ILogger<LoginController> logger)
       {
           _logger = logger;
           _tokenBuilder = tokenBuilder;
 
       }
       [HttpPost]
       public IActionResult Login([FromBody]LoginModel loginModel)
       {
           _logger.LogInformation($"{loginModel.UserName} login！");
           if (loginModel.UserName == "gsw" && loginModel.Password == "111111")
           {
               var claims = new Claim[] {
                   new Claim(ClaimTypes.Name, "gsw"),
                   new Claim(ClaimTypes.Role, "admin"),
                 
               };               
               var token = _tokenBuilder.BuildJwtToken(claims);
               _logger.LogInformation($"{loginModel.UserName} login success，and generate token return");
               return new JsonResult(new { Result = true, Data = token });
           }
           else
           {
               _logger.LogInformation($"{loginModel.UserName} login faile");
               return new JsonResult(new
               {
                   Result = false,
                   Message = "Authentication Failure"
               });
           }
       }
3、API项目中在Startup的ConfigureService方法中注入，并且在Controller或Action上加配置文件中的ProlicyName的配置名称，本例是permission

 services.AddApiJwtAuthorize((context) =>
 {
     //这里根据context中的Request和User来自定义权限验证，返回true为放行，返回fase时为拦截，其中User.Claims中有登录时自己定义的Claim
     return true;
 })

 [Authorize("permission")]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : Controller
    {
        //……
    }
 