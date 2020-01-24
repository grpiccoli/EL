using System.Collections.Generic;

namespace ELO.Data
{
    public class ClaimData
    {
        public static List<string> UserClaims { get; set; } = new List<string>
                                                            {
                                                                "Instituciones",
                                                                "Centros",
                                                                "Coordenadas",
                                                                "Producciones",
                                                                "Contactos",
                                                                "Usuarios",
                                                            };
    }
}
