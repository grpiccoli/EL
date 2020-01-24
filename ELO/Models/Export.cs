using System;
using System.ComponentModel.DataAnnotations;

namespace ELO.Models
{
    public class Export
    {
        public int Id { get; set; }

        public int RegionId { get; set; }

        public int CountryId { get; set; }

        [Display(Name = "Destino")]
        public virtual Country Country { get; set; }

        [Display(Name = "Origen")]
        public virtual Region Region { get; set; }

        [Display(Name = "Recurso")]
        public Species Species { get; set; }

        [Display(Name = "Procesamiento")]
        public Processing Processing { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Ton")]
        public int Kg { get; set; }

        [Display(Name = "FOB USD")]
        [DataType(DataType.Currency)]
        public int FOB { get; set; }
    }
    public enum Processing
    {
        Congelado,
        Conservas,
        Deshidratado,
        Fresco,
        Salado,
        Salazon,
        Secado,
        Seco,
        Vivos
    }
}
