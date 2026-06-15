using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using TravelNest.Services;
using Xunit;

namespace TravelNest.Tests
{
    public class ClassifierTests
    {
        [Fact]
        public async Task ClassifyAsync_ReturneazaCategorie_PentruCheltuialaValida()
        {
            var mockHttp = new MockHttpMessageHandler();
            var raspunsJson = JsonSerializer.Serialize(new[]
            {
                new { category = "Food", amount = 45.0 }
            });

            mockHttp.When("http://localhost:8001/classify")
                    .Respond("application/json", raspunsJson);

            var client = mockHttp.ToHttpClient();
            var service = new BudgetClassifierService(client);

            var result = await service.ClassifyAsync(new[]
            {
                ("Cina restaurant", 45m)
            });

            Assert.Single(result);
            Assert.Equal("Food", result[0].Category);
            Assert.Equal(45.0, result[0].Amount);
        }

        [Fact]
        public async Task ClassifyAsync_ReturneazaGoal_ListaGoala()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost:8001/classify")
                    .Respond("application/json", "[]");

            var client = mockHttp.ToHttpClient();
            var service = new BudgetClassifierService(client);

            var result = await service.ClassifyAsync(
                Enumerable.Empty<(string, decimal)>());

            Assert.Empty(result);
        }

        [Fact]
        public async Task ClassifyAsync_AgrupeazaSumele_PeCategorie()
        {
            var mockHttp = new MockHttpMessageHandler();
            var raspunsJson = JsonSerializer.Serialize(new[]
            {
                new { category = "Food", amount = 95.0 }
            });

            mockHttp.When("http://localhost:8001/classify")
                    .Respond("application/json", raspunsJson);

            var client = mockHttp.ToHttpClient();
            var service = new BudgetClassifierService(client);

            var result = await service.ClassifyAsync(new[]
            {
                ("Pizza", 45m),
                ("Cina", 50m)
            });

            Assert.Single(result);
            Assert.Equal(95.0, result[0].Amount);
        }

        [Fact]
        public async Task ClassifyAsync_MultipleCategori_ReturneazaToate()
        {
            var mockHttp = new MockHttpMessageHandler();
            var raspunsJson = JsonSerializer.Serialize(new[]
            {
                new { category = "Flights", amount = 200.0 },
                new { category = "Food", amount = 45.0 },
                new { category = "Tourism", amount = 30.0 }
            });

            mockHttp.When("http://localhost:8001/classify")
                    .Respond("application/json", raspunsJson);

            var client = mockHttp.ToHttpClient();
            var service = new BudgetClassifierService(client);

            var result = await service.ClassifyAsync(new[]
            {
                ("Zbor Bucuresti Paris", 200m),
                ("Cina restaurant", 45m),
                ("Bilet muzeu", 30m)
            });

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task ClassifyAsync_ServiceUnavailable_ThrowsException()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("http://localhost:8001/classify")
                    .Respond(HttpStatusCode.ServiceUnavailable);

            var client = mockHttp.ToHttpClient();
            var service = new BudgetClassifierService(client);

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.ClassifyAsync(new[] { ("Test", 10m) }));
        }
    }
}