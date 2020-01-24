using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Comuna
    {
        [Display(Name = "Código de Comuna")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        //Parent
        public int ProvinciaID { get; set; }

        public virtual Provincia Provincia { get; set; }

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
