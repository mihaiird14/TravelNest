using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class LocatieGrupTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }

        [Fact]
        public async Task AdaugaLocatie_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestAdaugaLocatie");

            var locatie = new LocatieGrup
            {
                Locatie = "Paris, France",
                GroupId = 1,
                CodTara = "FR",
                CheckIn = new DateOnly(2026, 7, 1),
                CheckOut = new DateOnly(2026, 7, 5)
            };

            context.LocatieGrups.Add(locatie);
            await context.SaveChangesAsync();

            var result = await context.LocatieGrups
                .FirstOrDefaultAsync(l => l.Locatie == "Paris, France");

            Assert.NotNull(result);
            Assert.Equal("FR", result.CodTara);
            Assert.Equal(new DateOnly(2026, 7, 1), result.CheckIn);
        }

        [Fact]
        public async Task AdaugaLocatieCuHotel_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestLocatieHotel");

            var locatie = new LocatieGrup
            {
                Locatie = "Roma, Italy",
                GroupId = 1,
                CodTara = "IT",
                HotelNume = "Hotel Colosseum",
                HotelLink = "https://booking.com/hotel/colosseum",
                CheckIn = new DateOnly(2026, 8, 10),
                CheckOut = new DateOnly(2026, 8, 15)
            };

            context.LocatieGrups.Add(locatie);
            await context.SaveChangesAsync();

            var result = await context.LocatieGrups
                .FirstOrDefaultAsync(l => l.Locatie == "Roma, Italy");

            Assert.NotNull(result);
            Assert.Equal("Hotel Colosseum", result.HotelNume);
            Assert.NotNull(result.HotelLink);
        }

        [Fact]
        public async Task StergeLocatie_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestStergeLocatie");

            var locatie = new LocatieGrup
            {
                Locatie = "Barcelona, Spain",
                GroupId = 1,
                CodTara = "ES"
            };

            context.LocatieGrups.Add(locatie);
            await context.SaveChangesAsync();

            context.LocatieGrups.Remove(locatie);
            await context.SaveChangesAsync();

            var result = await context.LocatieGrups
                .FirstOrDefaultAsync(l => l.Locatie == "Barcelona, Spain");

            Assert.Null(result);
        }

        [Fact]
        public async Task LocatiiGrup_Filtrate_DupaGrupId()
        {
            using var context = CreeazaContextInMemory("TestLocatiiFiltrate");

            context.LocatieGrups.AddRange(
                new LocatieGrup { Locatie = "Paris", GroupId = 1, CodTara = "FR" },
                new LocatieGrup { Locatie = "Lyon", GroupId = 1, CodTara = "FR" },
                new LocatieGrup { Locatie = "Berlin", GroupId = 2, CodTara = "DE" }
            );
            await context.SaveChangesAsync();

            var locatiiGrup1 = await context.LocatieGrups
                .Where(l => l.GroupId == 1)
                .CountAsync();

            Assert.Equal(2, locatiiGrup1);
        }

        [Fact]
        public async Task LocatieFaraHotel_HotelNumeNull()
        {
            using var context = CreeazaContextInMemory("TestLocatieFaraHotel");

            var locatie = new LocatieGrup
            {
                Locatie = "Amsterdam, Netherlands",
                GroupId = 1,
                CodTara = "NL"
            };

            context.LocatieGrups.Add(locatie);
            await context.SaveChangesAsync();

            var result = await context.LocatieGrups
                .FirstOrDefaultAsync(l => l.Locatie == "Amsterdam, Netherlands");

            Assert.NotNull(result);
            Assert.Null(result.HotelNume);
        }
    }
}