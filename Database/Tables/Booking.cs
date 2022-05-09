using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
    public class Booking
    {
        [Key]
        public Guid BookedId { get; set; } // same as reservation ID
        public int TravelId { get; set; }
        public int ReservedSeats { get; set; }
    }
}
