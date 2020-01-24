using System;

namespace ELO.Models
{
    public class Coordinate
    {
        //IDs
        public int ID { get; set; }
        public int? ComunaID { get; set; }
        public int? ProvinciaID { get; set; }
        public int? RegionID { get; set; }
        public int? CountryID { get; set; }
        public string StationID { get; set; }
        //Parents
        public virtual Comuna Comuna { get; set; }
        public virtual Provincia Provincia { get; set; }
        public virtual Region Region { get; set; }
        public virtual Country Country { get; set; }
        public virtual Station Station { get; set; }
        //ATT
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public int Vertex { get; set; }
        //CHILD
    }
}
