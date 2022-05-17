using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity


namespace Transport.Database.Tables
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Destination
    {
        [Key] // Defines it's a Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Defines it's generated, autoincremented value
        public int Id { get; set; } // When column is called *Id, it's by default a Primary Key unless specified otherwise
        public string Name { get; set; }
        public int Distance { get; set; }
    }
}
