using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class MesajTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }

        [Fact]
        public async Task AdaugaMesajGrup_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestMesajGrup");

            var mesaj = new Mesaj
            {
                ContinutContent = "Salut tuturor!",
                ExpeditorProfilId = 1,
                TravelGroupId = 1,
                DataTrimite = DateTime.Now
            };

            context.Mesaje.Add(mesaj);
            await context.SaveChangesAsync();

            var result = await context.Mesaje
                .FirstOrDefaultAsync(m => m.ContinutContent == "Salut tuturor!");

            Assert.NotNull(result);
            Assert.Equal(1, result.TravelGroupId);
            Assert.Null(result.DestinatarProfilId);
        }

        [Fact]
        public async Task AdaugaMesajPrivat_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestMesajPrivat");

            var mesaj = new Mesaj
            {
                ContinutContent = "Mesaj privat",
                ExpeditorProfilId = 1,
                DestinatarProfilId = 2,
                TravelGroupId = null,
                DataTrimite = DateTime.Now
            };

            context.Mesaje.Add(mesaj);
            await context.SaveChangesAsync();

            var result = await context.Mesaje
                .FirstOrDefaultAsync(m => m.ContinutContent == "Mesaj privat");

            Assert.NotNull(result);
            Assert.Equal(2, result.DestinatarProfilId);
            Assert.Null(result.TravelGroupId);
        }

        [Fact]
        public async Task EditeazaMesaj_UpdateCorect()
        {
            using var context = CreeazaContextInMemory("TestEditeazaMesaj");

            var mesaj = new Mesaj
            {
                ContinutContent = "Text initial",
                ExpeditorProfilId = 1,
                TravelGroupId = 1,
                DataTrimite = DateTime.Now
            };

            context.Mesaje.Add(mesaj);
            await context.SaveChangesAsync();

            mesaj.ContinutContent = "Text editat";
            await context.SaveChangesAsync();

            var result = await context.Mesaje.FindAsync(mesaj.Id);

            Assert.NotNull(result);
            Assert.Equal("Text editat", result.ContinutContent);
        }

        [Fact]
        public async Task StergeMesaj_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestStergeMesaj");

            var mesaj = new Mesaj
            {
                ContinutContent = "Mesaj de sters",
                ExpeditorProfilId = 1,
                TravelGroupId = 1,
                DataTrimite = DateTime.Now
            };

            context.Mesaje.Add(mesaj);
            await context.SaveChangesAsync();

            context.Mesaje.Remove(mesaj);
            await context.SaveChangesAsync();

            var result = await context.Mesaje
                .FirstOrDefaultAsync(m => m.ContinutContent == "Mesaj de sters");

            Assert.Null(result);
        }

        [Fact]
        public async Task MesajeGrup_Filtrate_DupaGrupId()
        {
            using var context = CreeazaContextInMemory("TestMesajeFiltrate");

            context.Mesaje.AddRange(
                new Mesaj { ContinutContent = "M1", ExpeditorProfilId = 1, TravelGroupId = 1, DataTrimite = DateTime.Now },
                new Mesaj { ContinutContent = "M2", ExpeditorProfilId = 2, TravelGroupId = 1, DataTrimite = DateTime.Now },
                new Mesaj { ContinutContent = "M3", ExpeditorProfilId = 1, TravelGroupId = 2, DataTrimite = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var mesajeGrup1 = await context.Mesaje
                .Where(m => m.TravelGroupId == 1)
                .CountAsync();

            Assert.Equal(2, mesajeGrup1);
        }

        [Fact]
        public void StergeMesaj_DupaFerestra_FlagExpired()
        {
            var dataTrimitere = DateTime.Now.AddMinutes(-15);
            var esteExpirat = (DateTime.Now - dataTrimitere).TotalMinutes > 10;

            Assert.True(esteExpirat);
        }
    }
}