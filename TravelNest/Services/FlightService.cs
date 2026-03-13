using Microsoft.Extensions.Options;
using TravelNest.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Globalization;
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
                throw new Exception(responseBody);
            }
        }
        return _accessToken;
    }
    public async Task<JsonDocument> SearchFlights(string plecare, string destinatie, string data)
    {
        var token = await GetTokenAmadeus();
        var url = $"https://test.api.amadeus.com/v2/shopping/flight-offers?originLocationCode={plecare}&destinationLocationCode={destinatie}&departureDate={data}&adults=1&max=10";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
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
    // kiwiAPi
    public async Task<List<ZborGrupuri>> cautaKiwi(string plecare, string destinatie, string data, int idGrup)
    {
        DateTime d = DateTime.Parse(data);
        string dataF = d.ToString("dd/MM/yyyy");
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://kiwi-cheap-flights.p.rapidapi.com/v2/search?fly_from={plecare}&fly_to={destinatie}&date_from={dataF}&date_to={dataF}&adults=1&curr=EUR");

        request.Headers.Add("X-RapidAPI-Key", "241f2057f8mshec8a00f76af14a1p190befjsne6ce4cf62cf7");
        request.Headers.Add("X-RapidAPI-Host", "kiwi-cheap-flights.p.rapidapi.com");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var rez = new List<ZborGrupuri>();

        if (doc.RootElement.TryGetProperty("data", out var dataList))
        {
            foreach (var z in dataList.EnumerateArray())
            {
                var ruta = z.GetProperty("route")[0];
                string codComp = z.GetProperty("airlines")[0].GetString();

                rez.Add(new ZborGrupuri
                {
                    GrupId = idGrup,
                    NumeCompanie = codComp,
                    NumarZbor = ruta.GetProperty("flight_no").ToString(),
                    Logo = $"https://images.kiwi.com/airlines/64/{codComp}.png",
                    OrasPlecare = z.GetProperty("cityFrom").GetString(),
                    OrasSosire = z.GetProperty("cityTo").GetString(),
                    AeroportPlecare = z.GetProperty("flyFrom").GetString(),
                    AeroportSosire = z.GetProperty("flyTo").GetString(),
                    DataPlecare = DateTime.Parse(z.GetProperty("local_departure").GetString()),
                    DataSosire = DateTime.Parse(z.GetProperty("local_arrival").GetString()),
                    Pret = z.GetProperty("price").GetDecimal(),
                    LinkZbor = z.GetProperty("deep_link").GetString()
                });
            }
        }
        return rez;
    }

    public List<ZborGrupuri> cautaAmadeus(JsonDocument doc, int idGrup, string orasPlecare, string orasSosire)
    {
        var rez = new List<ZborGrupuri>();
        var numeCompanii = new Dictionary<string, string>();
        if (doc.RootElement.TryGetProperty("dictionaries", out var dict) &&
            dict.TryGetProperty("carriers", out var carriers))
        {
            foreach (var prop in carriers.EnumerateObject())
                numeCompanii[prop.Name] = prop.Value.GetString();
        }

        if (doc.RootElement.TryGetProperty("data", out var dataList))
        {
            foreach (var z in dataList.EnumerateArray())
            {
                var itinerariu = z.GetProperty("itineraries")[0];
                var segment = itinerariu.GetProperty("segments")[0];
                string codComp = segment.GetProperty("carrierCode").GetString();
                string pretString = z.GetProperty("price").GetProperty("total").GetString();
                decimal pretCorect = decimal.Parse(pretString, CultureInfo.InvariantCulture);
                rez.Add(new ZborGrupuri
                {
                    GrupId = idGrup,
                    NumeCompanie = numeCompanii.ContainsKey(codComp) ? numeCompanii[codComp] : codComp,
                    NumarZbor = segment.GetProperty("number").GetString(),
                    Logo = $"https://images.kiwi.com/airlines/64/{codComp}.png",
                    OrasPlecare = orasPlecare,
                    OrasSosire = orasSosire,
                    AeroportPlecare = segment.GetProperty("departure").GetProperty("iataCode").GetString(),
                    AeroportSosire = segment.GetProperty("arrival").GetProperty("iataCode").GetString(),
                    DataPlecare = DateTime.Parse(segment.GetProperty("departure").GetProperty("at").GetString()),
                    DataSosire = DateTime.Parse(segment.GetProperty("arrival").GetProperty("at").GetString()),
                    Pret = pretCorect
                });
            }
        }
        return rez;
    }
}