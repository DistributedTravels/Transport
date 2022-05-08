using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TravelId { get; set; }
        public int ReservedSeats { get; set; }
        public Guid UserId { get; set; }
    }
}
