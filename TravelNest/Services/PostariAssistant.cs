using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TravelNest.Data;
using TravelNest.Models;

public class PostariAssistant
{
    private readonly ApplicationDbContext _context;
    private readonly GeminiService _gemini;
    private readonly HttpClient _httpClient;
    private readonly string _unsplashKey;

    public PostariAssistant(ApplicationDbContext context, GeminiService gemini,
                            HttpClient httpClient, IConfiguration config)
    {
        _context = context;
        _gemini = gemini;
        _httpClient = httpClient;
        _unsplashKey = config["Unsplash:AccessKey"]!;
    }

    public class PostareRezultat
    {
        public int? Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? Locatie { get; set; }
        public string? Descriere { get; set; }
        public bool DinBazaDate { get; set; }
    }

    public async Task<List<PostareRezultat>> GetPostariPentruVibe(List<string> orase, string promptOriginal, int total = 15)
    {
        //intai cautam postari dupa locatie + metadate
        // se exclud conturile private
        var oraseLower = orase.Select(o => o.ToLower()).ToList();
        //Console.WriteLine($"[DEBUG] Caut postari pentru orase: {string.Join(", ", oraseLower)}");
        //postari dupa locatie
        var postariDupaLocatie = await _context.Postares
            .Include(p => p.FisiereMedia)
            .Include(p => p.Profil)
            .Where(p => !p.Arhivata &&
                        p.FisiereMedia.Any() &&
                        !p.Profil.isPrivate &&
                        oraseLower.Any(o => p.Locatie != null && p.Locatie.ToLower().Contains(o)))
            .Take(25)
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Postari dupa locatie: {postariDupaLocatie.Count}");

        //postari dupa metadate
        var tagsDinPrompt = promptOriginal.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var postariDupaMetadate = await _context.Postares
            .Include(p => p.FisiereMedia)
            .Include(p => p.Profil)
            .Where(p => !p.Arhivata &&
                        p.FisiereMedia.Any() &&
                        !p.Profil.isPrivate &&
                        p.MetaDate != null &&
                        tagsDinPrompt.Any(t => p.MetaDate.ToLower().Contains(t)))
            .Take(25)
            .ToListAsync();

       //reuniune
        var postariCandidati = postariDupaLocatie
            .Union(postariDupaMetadate)
            .DistinctBy(p => p.Id)
            .ToList();
        var rezultate = new List<PostareRezultat>();
        var keywordsEngleza = await _gemini.GetKeywordsEngleza(promptOriginal);
        if (postariCandidati.Any())
        {
            var embeddingPrompt = await _gemini.GetEmbedding(promptOriginal);
            var scoruri = new List<(Postare postare, float scor)>();
            foreach (var p in postariCandidati)
            {
                var textPostare = $"{p.Locatie} {p.MetaDate} {p.Descriere}";
                var embeddingPostare = await _gemini.GetEmbedding(textPostare);
                var scor = _gemini.ScorCosinusSimilaritate(embeddingPrompt, embeddingPostare);
                scoruri.Add((p, scor));
            }

            var postariSortate = scoruri
                .OrderByDescending(x => x.scor)
                .Take(total)
                .ToList();

            foreach (var (postare, _) in postariSortate)
            {
                var primaImagine = postare.FisiereMedia.FirstOrDefault();
                rezultate.Add(new PostareRezultat
                {
                    Id = postare.Id,
                    ImageUrl = primaImagine?.Url,
                    Locatie = postare.Locatie,
                    Descriere = postare.Descriere,
                    DinBazaDate = true
                });
            }
        }
        //adaugam poze de pe unsplash daca nu s-au gasit macar 15 postari
        int lipsa = total - rezultate.Count;
        if (lipsa > 0)
        {
            var keywords = await _gemini.GetKeywordsEngleza(promptOriginal); 
            var imaginiUnsplash = await GetImaginiUnsplash(orase, promptOriginal, lipsa, keywords); 
            rezultate.AddRange(imaginiUnsplash);
        }
        return rezultate;
    }

    private async Task<List<PostareRezultat>> GetImaginiUnsplash(List<string> orase, string prompt, int count, string keywords = "travel")
    {
        var rezultate = new List<PostareRezultat>();
        if (count <= 0) return rezultate;

        try
        {
            //oras si vibe din prompt
            string orasCautat = orase.FirstOrDefault() ?? "travel";
            string vibeCautat = string.Join(" ", keywords.Split(' ').Take(2));
            string queryText = $"{orasCautat} {vibeCautat}".Trim();
            var query = Uri.EscapeDataString(queryText);
            //apel unsplash
            var url = $"https://api.unsplash.com/search/photos?query={query}&per_page={count}&client_id={_unsplashKey}";
            var raspunsRaw = await _httpClient.GetStringAsync(url);
            var raspuns = JsonSerializer.Deserialize<UnsplashRaspuns>(raspunsRaw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            //daca nu exista oras+ vibe, cauta doar dupa oras
            if (raspuns?.results == null || raspuns.results.Count == 0)
            {
                query = Uri.EscapeDataString(orasCautat);
                url = $"https://api.unsplash.com/search/photos?query={query}&per_page={count}&client_id={_unsplashKey}";
                raspunsRaw = await _httpClient.GetStringAsync(url);
                raspuns = JsonSerializer.Deserialize<UnsplashRaspuns>(raspunsRaw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // daca nu gaseste, cauta doar dupa vibe
            if (raspuns?.results == null || raspuns.results.Count == 0)
            {
                query = Uri.EscapeDataString(vibeCautat);
                url = $"https://api.unsplash.com/search/photos?query={query}&per_page={count}&client_id={_unsplashKey}";
                raspunsRaw = await _httpClient.GetStringAsync(url);
                raspuns = JsonSerializer.Deserialize<UnsplashRaspuns>(raspunsRaw,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            if (raspuns?.results != null)
            {
                foreach (var img in raspuns.results)
                {
                    rezultate.Add(new PostareRezultat
                    {
                        Id = null,
                        ImageUrl = img.urls?.regular,
                        Locatie = img.location?.name ?? orasCautat,
                        Descriere = img.description ?? img.alt_description ?? "Travel inspiration",
                        DinBazaDate = false
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return rezultate;
    }

    public class UnsplashRaspuns
    {
        public List<UnsplashImagine>? results { get; set; }
    }
    public class UnsplashImagine
    {
        public UnsplashUrls? urls { get; set; }
        public UnsplashLocation? location { get; set; }
        public string? description { get; set; }
        public string? alt_description { get; set; }
    }
    public class UnsplashUrls
    {
        public string? regular { get; set; }
    }
    public class UnsplashLocation
    {
        public string? name { get; set; }
    }
}