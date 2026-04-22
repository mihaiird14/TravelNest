using Microsoft.Extensions.AI;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Microsoft;
using System.Text.Json;
using TravelNest.Models;

public class GeminiService
{
    private readonly GenerativeModel _asistentAI;
    private readonly string _apiKey;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator; // ptr embeddings

    public GeminiService(IConfiguration config)
    {
        _apiKey = config["Gemini:ApiKey"];
        var googleAI = new GoogleAI(apiKey: _apiKey);
        _asistentAI = googleAI.GenerativeModel(model: "gemini-2.5-flash");
        _embeddingGenerator = new GeminiEmbeddingGenerator(apiKey: _apiKey, model: "text-embedding-004");
    }

    public class RezultatGenerare
    {
        public List<string> orase { get; set; } = new();
        public List<string> tags { get; set; } = new();
        public List<string> descrieri { get; set; } = new();
    }

    public async Task<RezultatGenerare?> GenerareDestinatii(string prompText, string? oraseExcluse = null)
    {
        try
        {
            var excludere = string.IsNullOrEmpty(oraseExcluse)
                ? ""
                : $"Do NOT suggest any of these cities: {oraseExcluse}. ";
            var cererePrompt = "First, evaluate the user's input. " +
                               $"User input: '{prompText}'. " +
                               "IF the input is offensive, toxic, nonsensical, or completely unrelated to travel and vacation planning, " +
                               "return ONLY an empty JSON object: {}. " +
                               "OTHERWISE, identify 5 specific cities and 5 vibe tags for the request. " +
                               excludere +
                               "Each city MUST have a major international airport in or very close to it. " +
                               "Return ONLY a raw JSON object with this exact structure: " +
                               "{ \"orase\": [\"City1\", \"City2\"...], \"descrieri\": [\"Short city description max 10 words for City1\", ...], \"tags\": [\"Tag1\", \"Tag2\"...] }. " +
                               "The descrieri array must contain a short description for each city (maximum 10 words each), in the same order as orase. " +
                               "No markdown, no talk, just the JSON.";

            var raspuns = await _asistentAI.GenerateContent(cererePrompt);
            var textRaspuns = raspuns.Text.Trim();
            if (textRaspuns == "{}" || string.IsNullOrEmpty(textRaspuns))
            {
                return null;
            }

            return ParsareDate(textRaspuns);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare AI: {ex.Message}");
            return null;
        }
    }
    //curatam json preventiv
    private RezultatGenerare ParsareDate(string? textRaw)
    {
        try
        {
            var jsonCurat = textRaw?.Replace("```json", "").Replace("```", "").Trim();
            return JsonSerializer.Deserialize<RezultatGenerare>(jsonCurat!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new RezultatGenerare();
        }
        catch
        {
            return new RezultatGenerare();
        }
    }
    public async Task<float[]> GetEmbedding(string text)
    {
        try
        {
            var googleAI = new GoogleAI(apiKey: _apiKey);
            var embModel = googleAI.GenerativeModel(model: "text-embedding-004");
            var rezultat = await embModel.EmbedContent(text);
            Console.WriteLine($"[DEBUG] Embedding reusit, valori: {rezultat?.Embedding?.Values?.Count ?? 0}"); // ✅
            return rezultat?.Embedding?.Values?.ToArray() ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Embedding EROARE: {ex.Message}"); 
            return Array.Empty<float>();
        }
    }


    public float ScorCosinusSimilaritate(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0) 
            return 0;
        float dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB) + 1e-8f);
    }
    //extrage din prompt cuvinte ptr unsplash
    public async Task<string> GetKeywordsEngleza(string prompt)
    {
        var cerere = $"Extract 1 travel vibe keyword in English from this text: '{prompt}'. " +
                     "Return ONLY the keyword separated by spaces, no punctuation.";
        var raspuns = await _asistentAI.GenerateContent(cerere);
        return raspuns.Text?.Trim() ?? "travel";
    }
    //prompt ptr itinerariu
    public async Task<List<ActivitateItinerariu>> GenerareItinerariu(int nrZile, string orase, string preferinte)
    {
        try
        {
            var cererePrompt = $"Generate a travel itinerary for a {nrZile}-day trip to {orase}. " +
                               $"User preferences: {preferinte}. " +
                               "Return ONLY a raw JSON array of objects with this exact structure: " +
                               "[{\"Zi\": 1, \"Ora\": \"HH:mm\", \"Titlu\": \"Activity Name\", \"Descriere\": \"Max 20 words description\"}]. " +
                               "Ensure the time is in 24h format (e.g. 09:00, 14:30). No markdown, no extra talk.";

            var raspuns = await _asistentAI.GenerateContent(cererePrompt);
            var textRaspuns = raspuns.Text.Trim().Replace("```json", "").Replace("```", "").Trim();

            return JsonSerializer.Deserialize<List<ActivitateItinerariu>>(textRaspuns,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ActivitateItinerariu>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare AI Itinerariu: {ex.Message}");
            return new List<ActivitateItinerariu>();
        }
    }
}