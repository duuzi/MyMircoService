using Api.ServiceB.IntegrationEvents.EventHandling;
using Api.ServiceB.IntegrationEvents.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toosame.EventBus.RabbitMQ;
using Toosame.EventBus.RabbitMQ.Extensions;

namespace Api.ServiceB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventBus(Configuration.GetSection("RabbitMQ").Get<RabbitMQOption>(),
                            eventHandlers =>
                            {
                                eventHandlers.AddEventHandler<AChangedIntegrationEventHandler>();
                            });
            services.AddControllers();
            #region 废弃
            ////注册rabbitMQ
            //services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            //{
            //    //var settings = sp.GetRequiredService<IOptions<CatalogSettings>>().Value;
            //    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

            //    var factory = new ConnectionFactory()
            //    {
            //        HostName = Configuration["EventBusConnection"],
            //        DispatchConsumersAsync = true
            //    };

            //    if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
            //    {
            //        factory.UserName = Configuration["EventBusUserName"];
            //    }

            //    if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
            //    {
            //        factory.Password = Configuration["EventBusPassword"];
            //    }

            //    var retryCount = 5;
            //    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
            //    {
            //        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
            //    }

            //    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            //});
            ////注册事件总线
            //services.AddSingleton<IEventBus, EventBusRabbitMQ.EventBusRabbitMQ>(sp =>
            //{
            //    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            //    //var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
            //    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ.EventBusRabbitMQ>>();
            //    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

            //    var retryCount = 5;
            //    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
            //    {
            //        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
            //    }

            //    return new EventBusRabbitMQ.EventBusRabbitMQ(rabbitMQPersistentConnection, logger, eventBusSubcriptionsManager, "ServiceA", retryCount);
            //});
            //services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseEventBus(eventBus =>
            {
                eventBus.Subscribe<AChangedEvent, AChangedIntegrationEventHandler>();
            });

            //ConfigureEventBus(app);
        }

        //private void ConfigureEventBus(IApplicationBuilder app)
        //{
        //    var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        //    eventBus.Subscribe<AChangedEvent, AChangedIntegrationEventHandler>();

        //}
    }
}
