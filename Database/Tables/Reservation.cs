﻿using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
    public class Reservation
    {
        [Key]
        public Guid ReserveId { get; set; }
        public int TravelId { get; set; }
        public int ReservedSeats { get; set; }
    }
}
