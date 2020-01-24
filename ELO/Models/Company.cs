﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class Company
    {
        [Display(Name = "RUT")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayFormat(DataFormatString = "{0,9:N0}" )]
        public int Id { get; set; }

        [Display(Name = "Business Name")]
        public string BsnssName { get; set; }

        [Display(Name = "Acronym")]
        public string Acronym { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Farming Centres")]
        public ICollection<Centre> Centres { get; set; }

        public ICollection<Publication> Publications { get; set; }
    }
}