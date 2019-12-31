using Api.ServiceB.IntegrationEvents.Events;
using Cp.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Api.ServiceB.IntegrationEvents.EventHandling
{
    public class AChangedIntegrationEventHandler : IIntegrationEventHandler<AChangedEvent>
    {
        private readonly ILogger<AChangedIntegrationEventHandler> _logger;

        public AChangedIntegrationEventHandler(ILogger<AChangedIntegrationEventHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           
        }
        public Task Handle(AChangedEvent @event)
        {
            _logger.LogInformation($"AchangeEvent触发:{@event.OldPrice}改变成{@event.NewPrice}了");
            return Task.CompletedTask;
        }
    }
}
