using Microsoft.Extensions.Options;
using TravelNest.Models;
using System.Net.Http.Headers;
using System.Text.Json;

public class HotelService
{
    private readonly HttpClient _httpClient;
    private readonly AmadeusSettings _settings;
    private string _accessToken;
    private readonly string _googleKey = "AIzaSyCfNJ41Gx7WZNP4K-UCaL775m9lYfPrBEg";
    public HotelService(HttpClient httpClient, IOptions<AmadeusSettings> options)
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
                throw new Exception("Eroare la obținerea token-ului Amadeus: " + responseBody);
            }
        }
        return _accessToken;
    }
    public async Task<JsonDocument> CautareHotelOras(string oras)
    {
        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query=hotels+in+{Uri.EscapeDataString(oras)}&type=lodging&key={_googleKey}";
        var raspuns = await _httpClient.GetAsync(url);
        var continut = await raspuns.Content.ReadAsStringAsync();
        return JsonDocument.Parse(continut);
    }
}