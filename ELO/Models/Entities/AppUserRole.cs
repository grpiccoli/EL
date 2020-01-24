using Microsoft.AspNetCore.Identity;

namespace ELO.Models
{
    public class AppUserRole : IdentityUserRole<string>
    {
        public string RoleAssigner { get; set; }
    }
}
