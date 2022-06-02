using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Reservation
    {

        [Key]
        public Guid ReserveId { get; set; }
        public int TravelId { get; set; }
        public Travel Travel { get; set; }
        public int ReservedSeats { get; set; }
        public ReservationState State { get; set; }
    }

    public enum ReservationState
    {
        RESERVED,
        UNRESERVED,
        PURCHASED,
        INVALIDATED
    }
}
