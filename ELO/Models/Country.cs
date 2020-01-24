using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Country
    {
        [Display(Name = "Código de País ISO Numerico")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int ContinentId { get; set; }
        public virtual Continent Continent { get; set; }
        public string Name { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public string Capital { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<Export> Exports { get; set; }
    }
}
