using System.Net.Http;
using System.Text;
using System.Text.Json;
using TravelNest.Models;

public class PythonFaceService
{
    private readonly HttpClient _http;

    public PythonFaceService(HttpClient http)
    {
        _http = http;
    }

    public async Task<FaceEmbeddingResponse?> GetEmbeddingsImagine(string imaginePath)
    {
        var body = new
        {
            imagine_path = imaginePath
        };

        string json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("http://localhost:5001/faceEmb", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<FaceEmbeddingResponse>(jsonResponse);
    }
}
