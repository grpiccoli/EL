using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Continent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string ISO { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<Country> Countries { get; set; }
    }
}
