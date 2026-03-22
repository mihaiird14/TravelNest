using System.Text.Json;
using TravelNest.Data;

namespace TravelNest.Services
{
    public class RecomandariForYou
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        public RecomandariForYou(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }
        public async Task<string> ObtineMetaDate(string descriere, string caleImagine)
        {
            try
            {
                using var continutCerere = new MultipartFormDataContent();
                continutCerere.Add(new StringContent(descriere ?? ""), "descriere");

                var dateImagine = await File.ReadAllBytesAsync(caleImagine);
                var continutImagine = new ByteArrayContent(dateImagine);
                continutImagine.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                continutCerere.Add(continutImagine, "fisier", Path.GetFileName(caleImagine));
                var raspuns = await _httpClient.PostAsync("http://127.0.0.1:8000/analizeazaPostare", continutCerere);
                if (raspuns.IsSuccessStatusCode)
                {
                    var rezultatJson = await raspuns.Content.ReadFromJsonAsync<JsonElement>();
                    if (rezultatJson.TryGetProperty("metadate", out var prop))
                    {
                        return prop.GetString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }
    }
}
