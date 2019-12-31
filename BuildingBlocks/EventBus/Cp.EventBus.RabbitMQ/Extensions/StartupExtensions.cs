using Cp.EventBus.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;


namespace Cp.EventBus.RabbitMQ.Extensions
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseEventBus(this IApplicationBuilder app,
            Action<IEventBus> subscribeOption)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            subscribeOption?.Invoke(eventBus);

            eventBus.StartSubscribe();

            return app;
        }

        public static void AddEventBus(this IServiceCollection services,
            RabbitMQOption rabbitMqOption,
            Action<ICollection<Type>> eventHandlerOption)
        {
            int port = 5672;
            string hostName = rabbitMqOption.EventBusConnection;

            if (rabbitMqOption.EventBusConnection.Contains(":"))
            {
                string[] hostPort = rabbitMqOption.EventBusConnection.Split(':');

                hostName = hostPort[0];
                port = Convert.ToInt32(hostPort[1]);
            }

            //添加RabbitMQ持久化连接单例
            services.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>(sp
                => new DefaultRabbitMQPersistentConnection(new ConnectionFactory()
                {
                    HostName = hostName,
                    Port = port,
                    UserName = rabbitMqOption.EventBusUserName,
                    Password = rabbitMqOption.EventBusPassword
                },
                sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>(),
                rabbitMqOption.EventBusRetryCount));

            var subscriptionClientName = rabbitMqOption.SubscriptionClientName;

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (rabbitMqOption.EventBusRetryCount > 0)
                {
                    retryCount = rabbitMqOption.EventBusRetryCount;
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection,
                    logger,
                    sp,
                    eventBusSubcriptionsManager,
                    rabbitMqOption.EventBusBrokeName,
                    subscriptionClientName,
                    retryCount);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            ICollection<Type> eventHandlers = new List<Type>();

            eventHandlerOption?.Invoke(eventHandlers);

            foreach (var handler in eventHandlers)
            {
                services.AddTransient(handler);
            }
        }

        public static void AddEventHandler<EH>(this ICollection<Type> types)
            where EH : class, IIntegrationEventHandler
        {
            types.Add(typeof(EH));
        }
    }
}
