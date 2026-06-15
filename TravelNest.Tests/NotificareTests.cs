using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class NotificareTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }

        [Fact]
        public async Task AdaugaNotificare_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestAdaugaNotificare");

            var notificare = new Notificare
            {
                TitluNotificare = "Follow nou",
                MesajNotificare = "Cineva te urmareste",
                TipNotificare = "follow",
                EsteCitita = false,
                destinatarId = 1,
                expeditorId = 2,
                DataTrimitere = DateTime.Now
            };

            context.Notificari.Add(notificare);
            await context.SaveChangesAsync();

            var result = await context.Notificari
                .FirstOrDefaultAsync(n => n.TitluNotificare == "Follow nou");

            Assert.NotNull(result);
            Assert.False(result.EsteCitita);
            Assert.Equal("follow", result.TipNotificare);
        }

        [Fact]
        public async Task MarcheazaCitita_UpdateCorect()
        {
            using var context = CreeazaContextInMemory("TestMarcheazaCitita");

            var notificare = new Notificare
            {
                TitluNotificare = "Like postare",
                MesajNotificare = "Cineva a apreciat postarea ta",
                TipNotificare = "like",
                EsteCitita = false,
                destinatarId = 1,
                expeditorId = 2,
                DataTrimitere = DateTime.Now
            };

            context.Notificari.Add(notificare);
            await context.SaveChangesAsync();

            notificare.EsteCitita = true;
            await context.SaveChangesAsync();

            var result = await context.Notificari
                .FirstOrDefaultAsync(n => n.TitluNotificare == "Like postare");

            Assert.NotNull(result);
            Assert.True(result.EsteCitita);
        }

        [Fact]
        public async Task NumarNecitite_Corect()
        {
            using var context = CreeazaContextInMemory("TestNumarNecitite");

            context.Notificari.AddRange(
                new Notificare { TitluNotificare = "N1", MesajNotificare = "m1", TipNotificare = "follow", EsteCitita = false, destinatarId = 1, expeditorId = 2, DataTrimitere = DateTime.Now },
                new Notificare { TitluNotificare = "N2", MesajNotificare = "m2", TipNotificare = "like", EsteCitita = false, destinatarId = 1, expeditorId = 3, DataTrimitere = DateTime.Now },
                new Notificare { TitluNotificare = "N3", MesajNotificare = "m3", TipNotificare = "comment", EsteCitita = true, destinatarId = 1, expeditorId = 4, DataTrimitere = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var necitite = await context.Notificari
                .Where(n => n.destinatarId == 1 && !n.EsteCitita)
                .CountAsync();

            Assert.Equal(2, necitite);
        }

        [Fact]
        public async Task StergeNotificare_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestStergeNotificare");

            var notificare = new Notificare
            {
                TitluNotificare = "Invitatie grup",
                MesajNotificare = "Ai fost invitat",
                TipNotificare = "invite",
                EsteCitita = false,
                destinatarId = 1,
                expeditorId = 2,
                DataTrimitere = DateTime.Now
            };

            context.Notificari.Add(notificare);
            await context.SaveChangesAsync();

            context.Notificari.Remove(notificare);
            await context.SaveChangesAsync();

            var result = await context.Notificari
                .FirstOrDefaultAsync(n => n.TitluNotificare == "Invitatie grup");

            Assert.Null(result);
        }
    }
}