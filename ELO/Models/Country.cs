using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ELO.Models
{
    public class Country
    {
        [Display(Name = "Código de País ISO Numerico")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public int ContinentID { get; set; }
        public virtual Continent Continent { get; set; }
        public string Name { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public string Capital { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public ICollection<Export> Exports { get; set; }
    }
}
