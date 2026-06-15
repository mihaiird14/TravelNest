using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class GroupTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }

        [Fact]
        public async Task CreareGrup_SalveazaCorect()
        {
            using var context = CreeazaContextInMemory("TestCreareGrup");

            var grup = new TravelGroup
            {
                Nume = "Vacanta Paris",
                AdminId = 1,
                Descriere = "Trip de vara",
                DataPlecare = new DateOnly(2026, 7, 1),
                DataIntoarcere = new DateOnly(2026, 7, 10)
            };

            context.TravelGroups.Add(grup);
            await context.SaveChangesAsync();

            var result = await context.TravelGroups
                .FirstOrDefaultAsync(g => g.Nume == "Vacanta Paris");

            Assert.NotNull(result);
            Assert.Equal(1, result.AdminId);
            Assert.Equal(new DateOnly(2026, 7, 1), result.DataPlecare);
        }

        [Fact]
        public async Task StergeGrup_EliminaCorect()
        {
            using var context = CreeazaContextInMemory("TestStergeGrup");

            var grup = new TravelGroup
            {
                Nume = "Trip de sters",
                AdminId = 1
            };

            context.TravelGroups.Add(grup);
            await context.SaveChangesAsync();

            context.TravelGroups.Remove(grup);
            await context.SaveChangesAsync();

            var result = await context.TravelGroups
                .FirstOrDefaultAsync(g => g.Nume == "Trip de sters");

            Assert.Null(result);
        }

        [Fact]
        public async Task AdaugaMembru_StatusPending()
        {
            using var context = CreeazaContextInMemory("TestMembruPending");

            var membru = new MembruGrup
            {
                ProfilId = 2,
                TravelGroupId = 1,
                Confirmare = "PENDING",
                DataInscrierii = DateTime.Now
            };

            context.MembruGrups.Add(membru);
            await context.SaveChangesAsync();

            var result = await context.MembruGrups
                .FirstOrDefaultAsync(m => m.ProfilId == 2 && m.TravelGroupId == 1);

            Assert.NotNull(result);
            Assert.Equal("PENDING", result.Confirmare);
        }

        [Fact]
        public async Task AcceptaMembru_StatusMember()
        {
            using var context = CreeazaContextInMemory("TestMembruAccepted");

            var membru = new MembruGrup
            {
                ProfilId = 2,
                TravelGroupId = 1,
                Confirmare = "PENDING",
                DataInscrierii = DateTime.Now
            };

            context.MembruGrups.Add(membru);
            await context.SaveChangesAsync();

            membru.Confirmare = "MEMBER";
            await context.SaveChangesAsync();

            var result = await context.MembruGrups
                .FirstOrDefaultAsync(m => m.ProfilId == 2 && m.TravelGroupId == 1);

            Assert.NotNull(result);
            Assert.Equal("MEMBER", result.Confirmare);
        }

        [Fact]
        public async Task EliminaMembru_StergeCorect()
        {
            using var context = CreeazaContextInMemory("TestEliminaMembru");

            var membru = new MembruGrup
            {
                ProfilId = 3,
                TravelGroupId = 1,
                Confirmare = "MEMBER",
                DataInscrierii = DateTime.Now
            };

            context.MembruGrups.Add(membru);
            await context.SaveChangesAsync();

            context.MembruGrups.Remove(membru);
            await context.SaveChangesAsync();

            var result = await context.MembruGrups
                .FirstOrDefaultAsync(m => m.ProfilId == 3 && m.TravelGroupId == 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task NumarMembri_Corect_DupaAdaugareMultipla()
        {
            using var context = CreeazaContextInMemory("TestNumarMembri");

            context.MembruGrups.AddRange(
                new MembruGrup { ProfilId = 1, TravelGroupId = 1, Confirmare = "MEMBER", DataInscrierii = DateTime.Now },
                new MembruGrup { ProfilId = 2, TravelGroupId = 1, Confirmare = "MEMBER", DataInscrierii = DateTime.Now },
                new MembruGrup { ProfilId = 3, TravelGroupId = 1, Confirmare = "PENDING", DataInscrierii = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var membriActivi = await context.MembruGrups
                .Where(m => m.TravelGroupId == 1 && m.Confirmare == "MEMBER")
                .CountAsync();

            Assert.Equal(2, membriActivi);
        }
    }
}