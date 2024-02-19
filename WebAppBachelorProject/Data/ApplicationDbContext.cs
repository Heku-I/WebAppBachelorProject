using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.Areas.Identity.Data;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<WebAppBachelorProjectUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        public DbSet<WebAppBachelorProjectUser> WebAppBachelorProjectUsers { get; set; }
        public DbSet<Image> Images { get; set; }

    }
}
