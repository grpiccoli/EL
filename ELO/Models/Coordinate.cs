using System;

namespace ELO.Models
{
    public class Coordinate
    {
        //Ids
        public int Id { get; set; }
        public int? CommuneId { get; set; }
        public int? ProvinceId { get; set; }
        public int? RegionId { get; set; }
        public int? CountryId { get; set; }
        public string StationId { get; set; }
        //Parents
        public virtual Commune Commune { get; set; }
        public virtual Province Province { get; set; }
        public virtual Region Region { get; set; }
        public virtual Country Country { get; set; }
        public virtual Station Station { get; set; }
        //ATT
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Vertex { get; set; }
        //CHILD
    }
}
