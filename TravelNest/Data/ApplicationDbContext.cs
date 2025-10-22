using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelNest.Models;

namespace TravelNest.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Profil> Profils { set; get; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Profil>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profil)
            .HasForeignKey<Profil>(a => a.UserId)
            .IsRequired();
    }
}