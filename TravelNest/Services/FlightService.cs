using Microsoft.Extensions.Options;
using TravelNest.Models;
using System.Net.Http.Headers;
using System.Text.Json;

public class FlightService
{
    private readonly HttpClient _httpClient;
    private readonly AmadeusSettings _settings;
    private string _accessToken;

    public FlightService(HttpClient httpClient, IOptions<AmadeusSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    private async Task<string> GetTokenAmadeus()
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            var values = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret }
        };
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://test.api.amadeus.com/v1/security/oauth2/token", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
            {
                _accessToken = tokenProp.GetString();
            }
            else
            {
                throw new Exception("Amadeus a refuzat autentificarea! Răspuns: " + responseBody);
            }
        }
        return _accessToken;
    }
    public async Task<JsonDocument> SearchFlights(string plecare, string destinatie, string data)
    {
        if (string.IsNullOrEmpty(_accessToken))
            await GetTokenAmadeus();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var url = $"https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode={plecare}&destinationLocationCode={destinatie}&departureDate={data}&adults=1&max=10";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
    public async Task<string> NextOras(double lat, double lon)
    {
        var token = await GetTokenAmadeus();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var url = $"https://test.api.amadeus.com/v1/reference-data/locations/airports?latitude={lat}&longitude={lon}&radius=500&page[limit]=1&sort=relevance";

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("data")[0].GetProperty("iataCode").GetString();
    }
    public async Task<string> GenerareCodAeroport(string cityName)
    {
        var token = await GetTokenAmadeus();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var url = $"https://test.api.amadeus.com/v1/reference-data/locations?subType=CITY,AIRPORT&keyword={Uri.EscapeDataString(cityName.Trim())}&page[limit]=10";
        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in data.EnumerateArray())
            {
                if (element.TryGetProperty("iataCode", out var iata) && !string.IsNullOrEmpty(iata.GetString()))
                {
                    return iata.GetString();
                }
                if (element.TryGetProperty("address", out var address))
                {
                    if (address.TryGetProperty("cityCode", out var city) && !string.IsNullOrEmpty(city.GetString()))
                    {
                        return city.GetString();
                    }
                }
            }
        }
        return null;
    }
}