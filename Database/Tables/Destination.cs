using System.ComponentModel.DataAnnotations; // for [Key]
using System.ComponentModel.DataAnnotations.Schema; // for Identity

namespace Transport.Database.Tables
{
    public class Destination
    {
        [Key] // Defines it's a Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Defines it's generated, autoincremented value
        public int Id { get; set; } // When column is called *Id, it's by default a Primary Key unless specified otherwise
        public string Name { get; set; }
        public int Distance { get; set; }
    }
}
