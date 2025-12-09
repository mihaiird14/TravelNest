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
    public DbSet<Postare> Postares { set; get; }
    public DbSet<FisierMedia> FisierMedias { set; get; }
    public DbSet<FaceEmbeddings> FaceEmbeddings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Profil>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profil)
            .HasForeignKey<Profil>(a => a.UserId)
            .IsRequired();
        modelBuilder.Entity<Postare>()
            .HasOne(p=> p.Profil)
            .WithMany(b=> b.Posts)
            .HasForeignKey(p=> p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FisierMedia>()
            .HasOne(f => f.Postare)
            .WithMany(p => p.FisiereMedia)
            .HasForeignKey(f => f.PostareId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FaceEmbeddings>()
            .HasOne(e => e.FisierMedia)
            .WithMany(m => m.FaceEmbeddings)
            .HasForeignKey(e => e.FisierMediaId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<FaceEmbeddings>()
            .HasOne(e => e.Person)
            .WithMany(p => p.FaceEmbeddings)
            .HasForeignKey(e => e.PersonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}