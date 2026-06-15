using Microsoft.EntityFrameworkCore;
using TravelNest.Models;

namespace TravelNest.Tests
{
    public class SimpleTestContext : DbContext
    {
        public SimpleTestContext(DbContextOptions<SimpleTestContext> options)
            : base(options)
        {
        }

        public DbSet<Cheltuiala> Cheltuieli { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<TravelGroup> TravelGroups { get; set; }
        public DbSet<MembruGrup> MembruGrups { get; set; }
        public DbSet<ZborGrupuri> ZborGrupuris { get; set; }
        public DbSet<Notificare> Notificari { get; set; }
        public DbSet<Mesaj> Mesaje { get; set; }
        public DbSet<LocatieGrup> LocatieGrups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cheltuiala>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Ignore(c => c.TravelGroup);
                entity.Ignore(c => c.PlatiMembri);
            });

            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Ignore(f => f.Follower);
                entity.Ignore(f => f.Followed);
            });

            modelBuilder.Entity<Profil>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Ignore(p => p.User);
                entity.Ignore(p => p.Posts);
                entity.Ignore(p => p.Followers);
                entity.Ignore(p => p.Following);
                entity.Ignore(p => p.MembruGrupuri);
                entity.Ignore(p => p.GrupuriAdministrate);
                entity.Ignore(p => p.FaceEmbeddings);
                entity.Ignore(p => p.NotificariPrimite);
                entity.Ignore(p => p.NotificariTrimise);
                entity.Ignore(p => p.MesajeTrimise);
                entity.Ignore(p => p.MesajePrimite);
                entity.Ignore(p => p.MesajeSeen);
                entity.Ignore(p => p.LikeComentarii);
                entity.Ignore(p => p.LikeReplyComentarii);
                entity.Ignore(p => p.ReplyComs);
            });

            modelBuilder.Entity<Mesaj>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Ignore(m => m.Expeditor);
                entity.Ignore(m => m.Destinatar);
                entity.Ignore(m => m.TravelGroup);
                entity.Ignore(m => m.VizualizariMessages);
            });

            modelBuilder.Entity<MembruGrup>(entity =>
            {
                entity.HasKey(mg => new { mg.ProfilId, mg.TravelGroupId });
                entity.Ignore(mg => mg.Profil);
                entity.Ignore(mg => mg.TravelGroup);
            });

            modelBuilder.Entity<TravelGroup>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Ignore(g => g.Admin);
                entity.Ignore(g => g.ListaParticipanti);
                entity.Ignore(g => g.Locatii);
                entity.Ignore(g => g.Documente);
                entity.Ignore(g => g.Zboruri);
                entity.Ignore(g => g.MesajeGrupGroup);
                entity.Ignore(g => g.ActivitatiItinerariu);
            });

            modelBuilder.Entity<ZborGrupuri>(entity =>
            {
                entity.HasKey(z => z.Id);
                entity.Ignore(z => z.Grup);
            });

            modelBuilder.Entity<Notificare>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Ignore(n => n.Destinatar);
                entity.Ignore(n => n.Expeditor);
            });

            modelBuilder.Entity<LocatieGrup>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Ignore(l => l.TravelGroup);
            });
        }
    }
}