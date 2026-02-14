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
    public DbSet<ReplyCom> ReplyComs { get; set; }
    public DbSet<LikeComentariu> LikeComentarii { get; set; }
    public DbSet<LikeReplyComentarii> LikeReplyComentarii { get; set; }
    public DbSet<TravelGroup> TravelGroups { get; set; }
    public DbSet<LocatieGrup> LocatieGrups { get; set; }
    public DbSet<MembruGrup> MembruGrups { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //cheie primara compusa ptr membru grup
        modelBuilder.Entity<MembruGrup>()
        .HasKey(mg => new { mg.ProfilId, mg.TravelGroupId });

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
        modelBuilder.Entity<ReplyCom>()
            .HasOne(r => r.Comentariu)
            .WithMany(c => c.Raspunsuri)
            .HasForeignKey(r => r.ComentariuId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ReplyCom>()
            .HasOne(r => r.User)
            .WithMany(u => u.ReplyComs)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LikeComentariu>()
            .HasOne(l => l.Comentariu)
            .WithMany(c => c.LikeComentariu)
            .HasForeignKey(l => l.ComentariuId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LikeComentariu>()
            .HasOne(l => l.Profil)
            .WithMany(p => p.LikeComentarii)
            .HasForeignKey(l => l.ProfilId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LikeReplyComentarii>()
            .HasOne(l=> l.ReplyCom)
            .WithMany(r => r.LikeReplyComentarii)
            .HasForeignKey(l => l.ReplyId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LikeReplyComentarii>()
            .HasOne(l => l.Profil)
            .WithMany(p => p.LikeReplyComentarii)
            .HasForeignKey(l => l.ProfilId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TravelGroup>()
            .HasOne(l=>l.Admin)
            .WithMany(p=>p.GrupuriAdministrate)
            .HasForeignKey(p=>p.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<LocatieGrup>()
            .HasOne(x=>x.TravelGroup)
            .WithMany(x=>x.Locatii)
            .HasForeignKey(x=>x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MembruGrup>()
               .HasOne(mg => mg.Profil)
               .WithMany(p => p.MembruGrupuri)
               .HasForeignKey(mg => mg.ProfilId)
               .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MembruGrup>()
            .HasOne(mg => mg.TravelGroup)
            .WithMany(tg => tg.ListaParticipanti) 
            .HasForeignKey(mg => mg.TravelGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}