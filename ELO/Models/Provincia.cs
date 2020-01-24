using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Provincia
    {
        [Display(Name = "Código de Provincia")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int RegionID { get; set; }

        public virtual Region Region { get; set; }

        [Display(Name = "Nombre de Provincia")]
        public string Name { get; set; }
        [Display(Name = "Superficie")]
        public int Surface { get; set; }
        [Display(Name = "Población")]
        public int Population { get; set; }

        public ICollection<Comuna> Comunas { get; set; }
        public ICollection<Coordinate> Coordinates { get; set; }
    }
}
