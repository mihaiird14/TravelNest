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
    public DbSet<SugestieTag> SugestieTags { get; set; }
    public DbSet<Comentariu> Comentarii { get; set; }
    public DbSet<LikesPostare> LikesPostari { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Comentariu>()
        .HasOne(c => c.Profil)
        .WithMany()
        .HasForeignKey(c => c.ProfilId)
        .OnDelete(DeleteBehavior.Restrict); 
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
        modelBuilder.Entity<SugestieTag>()
            .HasOne(s => s.FaceEmbedding)
            .WithMany() 
            .HasForeignKey(s => s.FaceEmbeddingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SugestieTag>()
            .HasOne(s => s.SuggestedPerson)
            .WithMany() 
            .HasForeignKey(s => s.SuggestedPersonId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LikesPostare>()
            .HasOne(l => l.Postare)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostareId)
            .OnDelete(DeleteBehavior.Cascade);
         modelBuilder.Entity<LikesPostare>()
            .HasOne(l => l.User)
            .WithMany() 
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}