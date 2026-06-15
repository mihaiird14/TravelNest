using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class ZborTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }

        [Fact]
        public async Task AdaugaZbor_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestAdaugaZbor");

            var zbor = new ZborGrupuri
            {
                GrupId = 1,
                NumeCompanie = "Wizz Air",
                NumarZbor = "W4 123",
                OrasPlecare = "Bucuresti",
                OrasSosire = "Paris",
                AeroportPlecare = "OTP",
                AeroportSosire = "CDG",
                DataPlecare = new DateTime(2026, 7, 1, 10, 0, 0),
                DataSosire = new DateTime(2026, 7, 1, 13, 0, 0),
                Pret = 150m,
                Logo = "wizzair.png"
            };

            context.ZborGrupuris.Add(zbor);
            await context.SaveChangesAsync();

            var result = await context.ZborGrupuris
                .FirstOrDefaultAsync(z => z.NumarZbor == "W4 123");

            Assert.NotNull(result);
            Assert.Equal("Wizz Air", result.NumeCompanie);
            Assert.Equal(150m, result.Pret);
        }

        [Fact]
        public async Task StergeZbor_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestStergeZbor");

            var zbor = new ZborGrupuri
            {
                GrupId = 1,
                NumeCompanie = "Ryanair",
                NumarZbor = "FR 456",
                OrasPlecare = "Cluj",
                OrasSosire = "Londra",
                AeroportPlecare = "CLJ",
                AeroportSosire = "STN",
                DataPlecare = new DateTime(2026, 8, 1, 8, 0, 0),
                DataSosire = new DateTime(2026, 8, 1, 11, 0, 0),
                Pret = 89m,
                Logo = "ryanair.png"
            };

            context.ZborGrupuris.Add(zbor);
            await context.SaveChangesAsync();

            context.ZborGrupuris.Remove(zbor);
            await context.SaveChangesAsync();

            var result = await context.ZborGrupuris
                .FirstOrDefaultAsync(z => z.NumarZbor == "FR 456");

            Assert.Null(result);
        }

        [Fact]
        public void DurataZbor_Calculata_Corect()
        {
            var dataPlecare = new DateTime(2026, 7, 1, 10, 0, 0);
            var dataSosire = new DateTime(2026, 7, 1, 13, 30, 0);

            var durata = dataSosire - dataPlecare;

            Assert.Equal(3.5, durata.TotalHours);
        }

        [Fact]
        public async Task ZboruriGrup_Filtrate_DupaGrupId()
        {
            using var context = CreeazaContextInMemory("TestZboruriFiltrate");

            context.ZborGrupuris.AddRange(
                new ZborGrupuri { GrupId = 1, NumeCompanie = "Wizz Air", NumarZbor = "W1", OrasPlecare = "OTP", OrasSosire = "CDG", AeroportPlecare = "OTP", AeroportSosire = "CDG", DataPlecare = DateTime.Now, DataSosire = DateTime.Now.AddHours(3), Pret = 100m, Logo = "w.png" },
                new ZborGrupuri { GrupId = 1, NumeCompanie = "Ryanair", NumarZbor = "R1", OrasPlecare = "CDG", OrasSosire = "BCN", AeroportPlecare = "CDG", AeroportSosire = "BCN", DataPlecare = DateTime.Now.AddDays(3), DataSosire = DateTime.Now.AddDays(3).AddHours(2), Pret = 80m, Logo = "r.png" },
                new ZborGrupuri { GrupId = 2, NumeCompanie = "Tarom", NumarZbor = "T1", OrasPlecare = "OTP", OrasSosire = "VIE", AeroportPlecare = "OTP", AeroportSosire = "VIE", DataPlecare = DateTime.Now, DataSosire = DateTime.Now.AddHours(2), Pret = 120m, Logo = "t.png" }
            );
            await context.SaveChangesAsync();

            var zboruriGrup1 = await context.ZborGrupuris
                .Where(z => z.GrupId == 1)
                .CountAsync();

            Assert.Equal(2, zboruriGrup1);
        }
    }
}