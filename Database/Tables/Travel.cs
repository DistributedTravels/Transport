using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
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
    }
}
