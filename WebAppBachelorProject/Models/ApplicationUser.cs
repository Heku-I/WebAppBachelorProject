using Microsoft.AspNetCore.Identity;

namespace WebAppBachelorProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserId { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }

        public bool Consent { get; set; } = false; 

    }
}
