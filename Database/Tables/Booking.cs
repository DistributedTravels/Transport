using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Booking
    {
        [Key]
        public Guid BookedId { get; set; } // same as reservation ID
        public int TravelId { get; set; }
        public Travel Travel { get; set; }
        public int ReservedSeats { get; set; }
    }
}
