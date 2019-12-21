using Toosame.EventBus.Events;

namespace Api.ServiceA.IntegrationEvents.Events
{
    public class AChangedEvent : IntegrationEvent
    {
        public int ProductId { get;  set; }

        public decimal NewPrice { get;  set; }

        public decimal OldPrice { get;  set; }

    }
}
