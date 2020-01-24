using System;
using System.ComponentModel.DataAnnotations;

namespace ELO.Models
{
    public class Arrival
    {
        //Id
        public int Id { get; set; }

        //PARent
        public int CommuneId { get; set; }

        public virtual Commune Commune { get; set; }

        //Att
        public string Caleta { get; set; }

        [Display(Name = "Fecha de Llegada")]
        [DisplayFormat(DataFormatString = "{0:d/M/yyyy}")]
        public DateTime Date { get; set; }

        [Display(Name = "Recurso")]
        public Species Species { get; set; }

        [Display(Name = "Toneladas")]
        [DisplayFormat(DataFormatString = "{0:0}")]
        public int Kg { get; set; }
    }
    public enum Species
    {
        Almeja,
        Erizo,
        Luga
    }
}
