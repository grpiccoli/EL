using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELO.Models
{
    public class AreaCode
    {
        [Display(Name = "Código de Área Telefónico")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public virtual ICollection<AreaCodeProvincia> AreaCodeProvincias { get; set; }
    }
}
