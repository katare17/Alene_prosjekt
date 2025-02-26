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

            // Konfigureer forholdet mellom GeoChange og WebUser
            modelBuilder.Entity<GeoChange>()
                .HasOne(g => g.User)
                .WithMany() // Antar at en bruker kan ha mange GeoChanges
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Valgfritt: Definer sletteadferd
        }
    }
}