﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELO.Models
{
    public enum Typep
    {
        Tesis, Articulo, Libro, Desconocido, Patente, Proyecto
    }
    public class Publication
    {
        //Ids
        public int Id { get; set; }
        public int CompanyId { get; set; }
        //Parent
        public virtual Company Company { get; set; }

        //Attributes

        [Display(Name = "Tipo")]
        public Typep Typep { get; set; }

        [Display(Name = "Fuente")]
        public string Source { get; set; }

        public string Uri { get; set; }

        [Display(Name = "Título")]
        public string Title { get; set; }

        public string Abbr { get; set; }

        [Display(Name = "Resumen")]
        public string Abstract { get; set; }

        [Display(Name = "Revista")]
        public string Journal { get; set; }

        [Display(Name = "DOI")]
        public string DOI { get; set; }

        [Display(Name = "Año")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy}")]
        public DateTime Date { get; set; }

        [Display(Name = "Autores")]
        public List<Author> Authors { get; set; }
    }
}
