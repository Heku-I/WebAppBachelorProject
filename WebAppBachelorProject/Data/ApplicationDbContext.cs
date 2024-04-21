﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.Areas.Identity.Data;
using ImageModel = WebAppBachelorProject.Models.Image;

namespace WebAppBachelorProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<WebAppBachelorProjectUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }


        public DbSet<WebAppBachelorProjectUser> WebAppBachelorProjectUsers { get; set; }
        public DbSet<ImageModel> Images { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.Ignore<Models.ImageDTO>();
        }


    }
}

