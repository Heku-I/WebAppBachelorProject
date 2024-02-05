using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.Models;

namespace WebAppBachelorProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated(); 
        }
        public DbSet<WebAppBachelorProject.Models.Image>? Image { get; set; }
    }
}
