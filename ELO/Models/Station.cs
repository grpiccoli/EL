using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Station
    {
        [Display(Name = "Código de Región")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        public int RegionID { get; set; }

        public virtual Region Region { get; set; }

        public string Area { get; set; }

        public string Name { get; set; }

        public Double Latitude { get; set; }

        public Double Longitude { get; set; }

        public ICollection<Coordinate> Coordinates { get; set; }
    }
}
