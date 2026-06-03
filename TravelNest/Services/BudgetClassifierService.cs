using System.Text;
using System.Text.Json;

namespace TravelNest.Services
{
    public class ExpenseCategoryDto
    {
        public string Category { get; set; } = "";
        public double Amount { get; set; }
    }

    public class BudgetClassifierService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:8001";

        public BudgetClassifierService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExpenseCategoryDto>> ClassifyAsync(
            IEnumerable<(string Title, decimal Amount)> expenses)
        {
            var payload = new
            {
                expenses = expenses.Select(e => new
                {
                    title = e.Title,
                    amount = (double)e.Amount
                })
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{BaseUrl}/classify", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ExpenseCategoryDto>>(result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<ExpenseCategoryDto>();
        }
    }
}