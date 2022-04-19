// https://github.com/dotnet-architecture/eShopOnContainers/blob/main/src/BuildingBlocks/EventBus/EventBus/Events/IntegrationEvent.cs
namespace Transport.Models
{
    // Class to inherit after for differnt types of events
    public class EventModel
    {
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }

        public EventModel()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
        }
    }
}
