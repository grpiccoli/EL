using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Commune
    {
        [Display(Name = "Código de Comuna")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        //Parent
        public int ProvinceId { get; set; }

        public virtual Province Province { get; set; }

        //ATt
        [Display(Name = "Nombre de Comuna")]
        public string Name { get; set; }

        [Display(Name = "Distrito Electoral")]
        public int DE { get; set; }

        [Display(Name = "Circunscripción Senatorial")]
        public int CS { get; set; }

        //Childs
        public ICollection<Coordinate> Coordinates { get; set; }
        public ICollection<Arrival> Arrivals { get; set; }
    }
}
