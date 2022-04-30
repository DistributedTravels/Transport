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
        public string Type { get; set; } // might be saved as int or string? - might get deleted if we opt out of coach travel type
        public int FreeSeats { get; set; }

        public string Source { get; set; } // name of departure place
        public string Destination { get; set; } // name of destination place
    }
}
