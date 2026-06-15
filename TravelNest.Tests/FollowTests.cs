using Microsoft.EntityFrameworkCore;
using TravelNest.Models;
using Xunit;

namespace TravelNest.Tests
{
    public class FollowTests
    {
        private SimpleTestContext CreeazaContextInMemory(string numeDb)
        {
            var options = new DbContextOptionsBuilder<SimpleTestContext>()
                .UseInMemoryDatabase(numeDb)
                .Options;
            return new SimpleTestContext(options);
        }
        [Fact]
        public async Task Follow_StatusPending_DupaTrimitere()
        {
            using var context = CreeazaContextInMemory("TestFollowPending");

            var follow = new Follow
            {
                FollowerId = 1,
                FollowedId = 2,
                Status = StatusUrmarire.Pending,
                DataCreat = DateTime.Now
            };

            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            var result = await context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == 1 && f.FollowedId == 2);

            Assert.NotNull(result);
            Assert.Equal(StatusUrmarire.Pending, result.Status);
        }

        [Fact]
        public async Task Follow_StatusAccepted_DupaAprobare()
        {
            using var context = CreeazaContextInMemory("TestFollowAccepted");

            var follow = new Follow
            {
                FollowerId = 1,
                FollowedId = 2,
                Status = StatusUrmarire.Pending,
                DataCreat = DateTime.Now
            };

            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            follow.Status = StatusUrmarire.Accepted;
            await context.SaveChangesAsync();

            var result = await context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == 1 && f.FollowedId == 2);

            Assert.NotNull(result);
            Assert.Equal(StatusUrmarire.Accepted, result.Status);
        }

        [Fact]
        public async Task Unfollow_EliminaRelatia()
        {
            using var context = CreeazaContextInMemory("TestUnfollow");

            var follow = new Follow
            {
                FollowerId = 1,
                FollowedId = 2,
                Status = StatusUrmarire.Accepted,
                DataCreat = DateTime.Now
            };

            context.Follows.Add(follow);
            await context.SaveChangesAsync();

            context.Follows.Remove(follow);
            await context.SaveChangesAsync();

            var result = await context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == 1 && f.FollowedId == 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task Follow_NuExistaRelatie_ReturneazaNull()
        {
            using var context = CreeazaContextInMemory("TestFollowNull");

            var result = await context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == 99 && f.FollowedId == 100);

            Assert.Null(result);
        }

        [Fact]
        public async Task Follow_NumarCorect_DupaAdaugareMultipla()
        {
            using var context = CreeazaContextInMemory("TestFollowMultiplu");

            context.Follows.AddRange(
                new Follow { FollowerId = 1, FollowedId = 2, Status = StatusUrmarire.Accepted, DataCreat = DateTime.Now },
                new Follow { FollowerId = 1, FollowedId = 3, Status = StatusUrmarire.Accepted, DataCreat = DateTime.Now },
                new Follow { FollowerId = 1, FollowedId = 4, Status = StatusUrmarire.Pending, DataCreat = DateTime.Now }
            );
            await context.SaveChangesAsync();

            var acceptate = await context.Follows
                .Where(f => f.FollowerId == 1 && f.Status == StatusUrmarire.Accepted)
                .CountAsync();

            Assert.Equal(2, acceptate);
        }
    }
}