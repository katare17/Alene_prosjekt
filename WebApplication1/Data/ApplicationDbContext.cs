using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> //DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<GeoChange> GeoChanges { get; set; }
    }

    public DbSet<AreaChange> AreaChanges { get; set; }
    //public DbSet<ApplicationUser> Users { get; set; }
}
