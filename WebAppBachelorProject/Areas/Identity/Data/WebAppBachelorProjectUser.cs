using Microsoft.AspNetCore.Identity;

namespace WebAppBachelorProject.Areas.Identity.Data
{
    public class WebAppBachelorProjectUser : IdentityUser
    {
        [PersonalData] public string? Firstname { get; set; }
        [PersonalData] public string? Lastname { get; set; }
    }
}

