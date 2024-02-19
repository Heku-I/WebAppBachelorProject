using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
           
        }


        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Image> Images { get; set; }

    }
}
