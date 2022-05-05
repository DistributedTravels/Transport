using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }
        public DateTime Time { get; set; }
        public string EventName { get; set; }
        public string Content { get; set; }
    }
}
