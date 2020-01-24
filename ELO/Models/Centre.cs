using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Centre
    {
        //Ids
        [Display(Name = "Concession Id")]
        [DisplayFormat(DataFormatString = "{0,7:N0}")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        //Parent
        [Display(Name = "RUT")]
        [DisplayFormat(DataFormatString = "{0,9:N0}")]
        public int CompanyId { get; set; }
        [Display(Name = "Company")]
        public virtual Company Company { get; set; }

        [Display(Name = "Commune")]
        public int ComunaId { get; set; }
        public virtual Comuna Comuna { get; set; }

        //Attributes
        [Display(Name = "Nombre del Centro")]
        public string Name { get; set; }

        [Display(Name = "Sigla o Acrónimo")]
        public string Acronym { get; set; }

        [Display(Name = "Folio No", Description ="National Registry of Aquaculture (RNA)")]
        public int? FolioRNA { get; set; }

        [Display(Name = "Type")]
        public CentreType Type { get; set; }

        [Display(Name = "Sitio web")]
        public string Url { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Cuerpo de Agua")]
        public CuerpoAgua? CuerpoAgua { get; set; }

        public bool? Certificable { get; set; }

        //Childs
        [Display(Name = "Coordinates")]
        public virtual ICollection<Coordinate> Coordinates { get; set; }

        //[Display(Name = "Annual productions")]
        //public virtual ICollection<Production> Productions { get; set; }
    }
    public enum CentreType
    {
        [Display(Name = "Farm Concession")]
        Cultivo = 1,
        [Display(Name = "Craft")]
        Embarcación = 2,
        [Display(Name = "Natural Bed")]
        BcoNatural = 3,
        [Display(Name = "Retailer")]
        Comecializadora = 4,
        [Display(Name = "Caleta")]
        Caleta = 4,
        [Display(Name = "Research Centre")]
        Investigación = 5,
        [Display(Name = "Planta")]
        Planta = 6
    }

    public enum CuerpoAgua
    {
        Río,
        Estero,
        Mar,
        Canal,
        Lago
    }
}
