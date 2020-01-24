using System.Collections.Generic;

namespace ELO.Data
{
    public class RoleData
    {
        public static List<string> AppRoles { get; set; } = new List<string>
                                                            {
                                                                "Administrador",
                                                                "Editor",
                                                                "Invitado"
                                                            };
    }
}
