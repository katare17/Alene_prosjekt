using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<WebUser> //DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GeoChange> GeoChanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship between GeoChange and WebUser 
            modelBuilder.Entity<GeoChange>()
                .HasOne(g => g.User)
                .WithMany() // Assuming a user can have many GeoChanges
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Define delete behavior
        }
    }
}