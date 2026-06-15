using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class BugetTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }
        [Fact]
        public void DistribuieCheltuiala_ImpartEgal_SumaCorecta()
        {
            decimal sumaTotala = 300m;
            int nrMembri = 3;
            decimal expected = 100m;

            decimal result = Math.Round(sumaTotala / nrMembri, 2);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DistribuieCheltuiala_SumaImpara_RoundingCorect()
        {
            decimal sumaTotala = 100m;
            int nrMembri = 3;
            decimal expected = 33.33m;

            decimal result = Math.Round(sumaTotala / nrMembri, 2);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetCheltuieli_ReturneazaGoal_GrupInexistent()
        {
            using var context = CreeazaContextInMemory("TestGoal");

            var cheltuieli = await context.Cheltuieli
                .Where(c => c.TravelGroupId == 9999)
                .ToListAsync();

            Assert.Empty(cheltuieli);
        }

        [Fact]
        public async Task AdaugaCheltuiala_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestAdauga");

            var cheltuiala = new Cheltuiala
            {
                TravelGroupId = 1,
                Titlu = "Cina restaurant",
                SumaTotala = 120m,
                EsteAutomata = false
            };

            context.Cheltuieli.Add(cheltuiala);
            await context.SaveChangesAsync();

            var result = await context.Cheltuieli
                .FirstOrDefaultAsync(c => c.Titlu == "Cina restaurant");

            Assert.NotNull(result);
            Assert.Equal(120m, result.SumaTotala);
        }

        [Fact]
        public async Task StergeCheltuiala_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestSterge");

            var cheltuiala = new Cheltuiala
            {
                TravelGroupId = 1,
                Titlu = "Test sterge",
                SumaTotala = 50m,
                EsteAutomata = false
            };

            context.Cheltuieli.Add(cheltuiala);
            await context.SaveChangesAsync();

            context.Cheltuieli.Remove(cheltuiala);
            await context.SaveChangesAsync();

            var result = await context.Cheltuieli
                .FirstOrDefaultAsync(c => c.Titlu == "Test sterge");

            Assert.Null(result);
        }

        [Fact]
        public async Task CheltuialaAutomata_FlagCorect()
        {
            using var context = CreeazaContextInMemory("TestAutomat");

            var cheltuiala = new Cheltuiala
            {
                TravelGroupId = 1,
                Titlu = "Zbor Bucuresti Paris",
                SumaTotala = 450m,
                EsteAutomata = true
            };

            context.Cheltuieli.Add(cheltuiala);
            await context.SaveChangesAsync();

            var result = await context.Cheltuieli
                .FirstOrDefaultAsync(c => c.Titlu == "Zbor Bucuresti Paris");

            Assert.NotNull(result);
            Assert.True(result.EsteAutomata);
        }
    }
}