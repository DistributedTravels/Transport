using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Travel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime DepartureTime { get; set; }
        public int FreeSeats { get; set; }
        public string Source { get; set; } // name of departure place
        public string Destination { get; set; } // name of destination place
        public double Price { get; set; }

        public List<Reservation> Reservations { get; set; }
    }
}
