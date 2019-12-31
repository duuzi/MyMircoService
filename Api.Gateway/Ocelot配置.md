#集成网关
##在asp.net core 2.0里通过nuget即可完成集成，或者命令行dotnet add package Ocelot以及通过vs2017 UI添加Ocelot nuget引用都可以。

Install-Package Ocelot
#配置
我们需要添加一个.json的文件用来添加Ocelot的配置，以下是最基本的配置信息。

{
    "ReRoutes": [],
    "GlobalConfiguration": {
        "BaseUrl": "https://api.mybusiness.com"
    }
}
要特别注意一下BaseUrl是我们外部暴露的Url，比如我们的Ocelot运行在http://123.111.1.1的一个地址上，但是前面有一个 nginx绑定了域名http://api.jessetalk.cn，那这里我们的BaseUrl就是 http://api.jessetalk.cn。

###将配置文件加入ASP.NET Core Configuration

我们需要通过WebHostBuilder将我们添加的json文件添加进asp.net core的配置

public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration( (hostingContext,builder) => {
            builder
            .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
            .AddJsonFile("Ocelot.json");
        })
        .UseStartup<Startup>()
        .Build();
 

###配置依赖注入与中间件
在startup.cs中我们首先需要引用两个命名空间

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
接下来就是添加依赖注入和中间件

public void ConfigureServices(IServiceCollection services)
{
    services.AddOcelot();
}
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseOcelot().Wait();
}
###Ocelot功能介绍
通过配置文件可以完成对Ocelot的功能配置：路由、服务聚合、服务发现、认证、鉴权、限流、熔断、缓存、Header头传递等。在配置文件中包含两个根节点：ReRoutes和GlobalConfiguration。ReRoutes是一个数组，其中的每一个元素代表了一个路由，我们可以针对每一个路由进行以上功能配置。下面是一个完整的路由配置

{
          "DownstreamPathTemplate": "/",
          "UpstreamPathTemplate": "/",
          "UpstreamHttpMethod": [
              "Get"
          ],
          "AddHeadersToRequest": {},
          "AddClaimsToRequest": {},
          "RouteClaimsRequirement": {},
          "AddQueriesToRequest": {},
          "RequestIdKey": "",
          "FileCacheOptions": {
              "TtlSeconds": 0,
              "Region": ""
          },
          "ReRouteIsCaseSensitive": false,
          "ServiceName": "",
          "DownstreamScheme": "http",
          "DownstreamHostAndPorts": [
              {
                  "Host": "localhost",
                  "Port": 51876,
              }
          ],
          "QoSOptions": {
              "ExceptionsAllowedBeforeBreaking": 0,
              "DurationOfBreak": 0,
              "TimeoutValue": 0
          },
          "LoadBalancer": "",
          "RateLimitOptions": {
              "ClientWhitelist": [],
              "EnableRateLimiting": false,
              "Period": "",
              "PeriodTimespan": 0,
              "Limit": 0
          },
          "AuthenticationOptions": {
              "AuthenticationProviderKey": "",
              "AllowedScopes": []
          },
          "HttpHandlerOptions": {
              "AllowAutoRedirect": true,
              "UseCookieContainer": true,
              "UseTracing": true
          },
          "UseServiceDiscovery": false
      }
Downstream是下游服务配置
UpStream是上游服务配置
Aggregates 服务聚合配置
ServiceName, LoadBalancer, UseServiceDiscovery 配置服务发现
AuthenticationOptions 配置服务认证
RouteClaimsRequirement 配置Claims鉴权
RateLimitOptions为限流配置
FileCacheOptions 缓存配置
QosOptions 服务质量与熔断
DownstreamHeaderTransform头信息转发
我们接下来将对这些功能一一进行介绍和配置

#路由
路由是API网关最基本也是最核心的功能、ReRoutes下就是由多个路由节点组成。

{
    "ReRoutes": [
    ]
}
而每一个路由由以下几个基本信息组成：

下面这个配置信息就是将用户的请求 /post/1 转发到 localhost/api/post/1

{
    "DownstreamPathTemplate": "/api/post/{postId}",
    "DownstreamScheme": "https",
    "DownstreamHostAndPorts": [
            {
                "Host": "localhost",
                "Port": 80,
            }
        ],
    "UpstreamPathTemplate": "/post/{postId}",
    "UpstreamHttpMethod": [ "Get"]
}
DownstreamPathTemplate：下游戏
DownstreamScheme：下游服务http schema
DownstreamHostAndPorts：下游服务的地址，如果使用LoadBalancer的话这里可以填多项
UpstreamPathTemplate: 上游也就是用户输入的请求Url模板
UpstreamHttpMethod: 上游请求http方法，可使用数组
###万能模板

万能模板即所有请求全部转发，UpstreamPathTemplate 与DownstreamPathTemplate 设置为 “/{url}”

{
    "DownstreamPathTemplate": "/{url}",
    "DownstreamScheme": "https",
    "DownstreamHostAndPorts": [
            {
                "Host": "localhost",
                "Port": 80,
            }
        ],
    "UpstreamPathTemplate": "/{url}",
    "UpstreamHttpMethod": [ "Get" ]
}
万能模板的优先级最低，只要有其它的路由模板，其它的路由模板则会优先生效。

###上游Host
上游Host也是路由用来判断的条件之一，由客户端访问时的Host来进行区别。比如当a.jesetalk.cn/users/{userid}和b.jessetalk.cn/users/{userid}两个请求的时候可以进行区别对待。

{
    "DownstreamPathTemplate": "/",
    "DownstreamScheme": "https",
    "DownstreamHostAndPorts": [
            {
                "Host": "10.0.10.1",
                "Port": 80,
            }
        ],
    "UpstreamPathTemplate": "/",
    "UpstreamHttpMethod": [ "Get" ],
    "UpstreamHost": "a.jessetalk.cn"
}
###Prioirty优先级
对多个产生冲突的路由设置优化级

{
    "UpstreamPathTemplate": "/goods/{catchAll}"
    "Priority": 0
}
{
    "UpstreamPathTemplate": "/goods/delete"
    "Priority": 1
}
比如你有同样两个路由，当请求/goods/delete的时候，则下面那个会生效。也就是说Prority是大的会被优先选择。

###路由负载均衡
当下游服务有多个结点的时候，我们可以在DownstreamHostAndPorts中进行配置。

{
    "DownstreamPathTemplate": "/api/posts/{postId}",
    "DownstreamScheme": "https",
    "DownstreamHostAndPorts": [
            {
                "Host": "10.0.1.10",
                "Port": 5000,
            },
            {
                "Host": "10.0.1.11",
                "Port": 5000,
            }
        ],
    "UpstreamPathTemplate": "/posts/{postId}",
    "LoadBalancer": "LeastConnection",
    "UpstreamHttpMethod": [ "Put", "Delete" ]
}
LoadBalancer将决定负载均衡的算法

LeastConnection – 将请求发往最空闲的那个服务器
RoundRobin – 轮流发送
NoLoadBalance – 总是发往第一个请求或者是服务发现
在负载均衡这里，我们还可以和Consul结合来使用服务发现，我们将在后面的小节中进行详述。

#请求聚合
即将多个API请求结果合并为一个返回。要实现请求聚合我们需要给其它参与的路由起一个Key。

{
    "ReRoutes": [
        {
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/laura",
            "UpstreamHttpMethod": [
                "Get"
            ],
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51881
                }
            ],
            "Key": "Laura"
        },
        {
            "DownstreamPathTemplate": "/",
            "UpstreamPathTemplate": "/tom",
            "UpstreamHttpMethod": [
                "Get"
            ],
            "DownstreamScheme": "http",
            "DownstreamHostAndPorts": [
                {
                    "Host": "localhost",
                    "Port": 51882
                }
            ],
            "Key": "Tom"
        }
    ],
    "Aggregates": [
        {
            "ReRouteKeys": [
                "Tom",
                "Laura"
            ],
            "UpstreamPathTemplate": "/"
        }
    ]
}
当我们请求/的时候，会将/tom和/laura两个结果合并到一个response返回

{"Tom":{"Age": 19},"Laura":{"Age": 25}}
需要注意的是：

聚合服务目前只支持返回json
目前只支持Get方式请求下游服务
任何下游的response header并会被丢弃
如果下游服务返回404，聚合服务只是这个key的value为空，它不会返回404
有一些其它的功能会在将来实现

下游服务很慢的处理
做一些像 GraphQL的处理对下游服务返回结果进行处理
404的处理
#限流
对请求进行限流可以防止下游服务器因为访问过载而崩溃，这个功能就是我们的张善友张队进添加进去的。非常优雅的实现，我们只需要在路由下加一些简单的配置即可以完成。

"RateLimitOptions": {
    "ClientWhitelist": [],
    "EnableRateLimiting": true,
    "Period": "1s",
    "PeriodTimespan": 1,
    "Limit": 1
}
ClientWihteList 白名单
EnableRateLimiting 是否启用限流
Period 统计时间段：1s, 5m, 1h, 1d
PeroidTimeSpan 多少秒之后客户端可以重试
Limit 在统计时间段内允许的最大请求数量
在 GlobalConfiguration下我们还可以进行以下配置

"RateLimitOptions": {
  "DisableRateLimitHeaders": false,
  "QuotaExceededMessage": "Customize Tips!",
  "HttpStatusCode": 999,
  "ClientIdHeader" : "Test"
}
Http头  X-Rate-Limit 和 Retry-After 是否禁用
QuotaExceedMessage 当请求过载被截断时返回的消息
HttpStatusCode 当请求过载被截断时返回的http status
ClientIdHeader 用来识别客户端的请求头，默认是 ClientId
#服务质量与熔断
熔断的意思是停止将请求转发到下游服务。当下游服务已经出现故障的时候再请求也是功而返，并且增加下游服务器和API网关的负担。这个功能是用的Pollly来实现的，我们只需要为路由做一些简单配置即可

"QoSOptions": {
    "ExceptionsAllowedBeforeBreaking":3,
    "DurationOfBreak":5,
    "TimeoutValue":5000
}
ExceptionsAllowedBeforeBreaking 允许多少个异常请求
DurationOfBreak 熔断的时间，单位为秒
TimeoutValue 如果下游请求的处理时间超过多少则自如将请求设置为超时
#缓存
Ocelot可以对下游请求结果进行缓存 ，目前缓存的功能还不是很强大。它主要是依赖于CacheManager 来实现的，我们只需要在路由下添加以下配置即可

"FileCacheOptions": { "TtlSeconds": 15, "Region": "somename" }
Region是对缓存进行的一个分区，我们可以调用Ocelot的 administration API来移除某个区下面的缓存 。

#认证
如果我们需要对下游API进行认证以及鉴权服务的，则首先Ocelot 网关这里需要添加认证服务。这和我们给一个单独的API或者ASP.NET Core Mvc添加认证服务没有什么区别。

public void ConfigureServices(IServiceCollection services)
{
    var authenticationProviderKey = "TestKey";

    services.AddAuthentication()
        .AddJwtBearer(authenticationProviderKey, x =>
        {
        });
}
然后在ReRoutes的路由模板中的AuthenticationOptions进行配置，只需要我们的AuthenticationProviderKey一致即可。

"ReRoutes": [{
        "DownstreamHostAndPorts": [
            {
                "Host": "localhost",
                "Port": 51876,
            }
        ],
        "DownstreamPathTemplate": "/",
        "UpstreamPathTemplate": "/",
        "UpstreamHttpMethod": ["Post"],
        "ReRouteIsCaseSensitive": false,
        "DownstreamScheme": "http",
        "AuthenticationOptions": {
            "AuthenticationProviderKey": "TestKey",
            "AllowedScopes": []
        }
    }]
###JWT Tokens

要让网关支持JWT 的认证其实和让API支持JWT  Token的认证是一样的

public void ConfigureServices(IServiceCollection services)
{
    var authenticationProviderKey = "TestKey";

    services.AddAuthentication()
        .AddJwtBearer(authenticationProviderKey, x =>
        {
            x.Authority = "test";
            x.Audience = "test";
        });

    services.AddOcelot();
}
###Identity Server Bearer Tokens

添加Identity Server的认证也是一样

public void ConfigureServices(IServiceCollection services)
{
    var authenticationProviderKey = "TestKey";
    var options = o =>
        {
            o.Authority = "https://whereyouridentityserverlives.com";
            o.ApiName = "api";
            o.SupportedTokens = SupportedTokens.Both;
            o.ApiSecret = "secret";
        };

    services.AddAuthentication()
        .AddIdentityServerAuthentication(authenticationProviderKey, options);

    services.AddOcelot();
}
###Allowed Scopes

这里的Scopes将从当前 token 中的 claims中来获取，我们的鉴权服务将依靠于它来实现 。当前路由的下游API需要某个权限时，我们需要在这里声明 。和oAuth2中的 scope意义一致。

#鉴权
我们通过认证中的AllowedScopes 拿到claims之后，如果要进行权限的鉴别需要添加以下配置

"RouteClaimsRequirement": {
    "UserType": "registered"
}
当前请求上下文的token中所带的claims如果没有 name=”UserType” 并且 value=”registered” 的话将无法访问下游服务。

#请求头转化
请求头转发分两种：转化之后传给下游和从下游接收转化之后传给客户端。在Ocelot的配置里面叫做Pre  Downstream Request和Post Downstream Request。目前的转化只支持查找和替换。我们用到的配置主要是 UpstreamHeaderTransform 和 DownstreamHeaderTransform

###Pre Downstream Request

"Test": "http://www.bbc.co.uk/, http://ocelot.com/"
比如我们将客户端传过来的Header中的 Test 值改为 http://ocelot.com/之后再传给下游

 "UpstreamHeaderTransform": {
    "Test": "http://www.bbc.co.uk/, http://ocelot.com/"
},
###Post Downstream Request

而我们同样可以将下游Header中的Test再转为 http://www.bbc.co.uk/之后再转给客户端。

"DownstreamHeaderTransform": {
    "Test": "http://www.bbc.co.uk/, http://ocelot.com/"
},
###变量

在请求头转化这里Ocelot为我们提供了两个变量：BaseUrl和DownstreamBaseUrl。BaseUrl就是我们在GlobalConfiguration里面配置的BaseUrl，后者是下游服务的Url。这里用301跳转做一个示例如何使用这两个变量。

默认的301跳转，我们会返回一个Location的头，于是我们希望将http://www.bbc.co.uk 替换为 http://ocelot.com，后者者网关对外的域名。

"DownstreamHeaderTransform": {
    "Location": "http://www.bbc.co.uk/, http://ocelot.com/"
},
 "HttpHandlerOptions": {
    "AllowAutoRedirect": false,
},
我们通过DownstreamHeaderTranfrom将下游返回的请求头中的Location替换为了网关的域名，而不是下游服务的域名。所以在这里我们也可以使用BaseUrl来做为变量替换。

"DownstreamHeaderTransform": {
    "Location": "http://localhost:6773, {BaseUrl}"
},
 "HttpHandlerOptions": {
    "AllowAutoRedirect": false,
},
当我们的下游服务有多个的时候，我们就没有办法找到前面的那个http://localhost:6773，因为它可能是多个值。所以这里我们可以使用DownstreamBaseUrl。

"DownstreamHeaderTransform": {
    "Location": "{DownstreamBaseUrl}, {BaseUrl}"
},
 "HttpHandlerOptions": {
    "AllowAutoRedirect": false,
},
#Claims转化
Claims转化功能可以将Claims中的值转化到请求头、Query String、或者下游的Claims中，对于Claims的转化，比较特殊的一点是它提供了一种对字符串进行解析的方法。举个例子，比如我们有一个sub的claim。这个claims的 name=”sub” value=”usertypevalue|useridvalue”，实际上我们不会弄这么复杂的value，它是拼接来的，但是我们为了演示这个字符串解析的功能，所以使用了这么一个复杂的value。

Ocelot为我们提供的功能分为三段，第一段是Claims[sub]，很好理解[] 里面是我们的claim的名称。第二段是  > 表示对字符串进行拆分, 后面跟着拆分完之后我们要取的那个数组里面的某一个元素用 value[index]来表示，取第0位元素也可以直接用value。第三段也是以 > 开头后面跟着我们的分隔符，在我们上面的例子分隔符是 |

所以在这里如果我们要取  usertype这个claim就会这样写： Claims[sub] > value[0] > |

Claim取到之后我们如果要放到请求头、QueryString、以及Claim当中对应有以下三个配置。

###Claims to Claims 

"AddClaimsToRequest": {
    "UserType": "Claims[sub] > value[0] > |",
    "UserId": "Claims[sub] > value[1] > |"
}
###Claims to Headers 

"AddHeadersToRequest": {
    "CustomerId": "Claims[sub] > value[1] > |"
}
这里我们还是用的上面那个 sub = usertypevalue|useridvalue 的claim来进行处理和转化。

###Claims to Query String

"AddQueriesToRequest": {
    "LocationId": "Claims[LocationId] > value",
}
这里没有进行分隔，所以直接取了value。
