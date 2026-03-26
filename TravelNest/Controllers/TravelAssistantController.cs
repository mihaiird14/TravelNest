using Microsoft.AspNetCore.Mvc;
using TravelNest.Data;

namespace TravelNest.Controllers
{
    public class TravelAssistantController : Controller
    {
        private readonly GeminiService _serviciuGemini;
        private readonly ApplicationDbContext _context;
        private readonly PostariAssistant _postariAssistant; //ptr sugestii postari
        public TravelAssistantController(GeminiService serviciuGemini, ApplicationDbContext context, PostariAssistant postariAssistant)
        {
            _serviciuGemini = serviciuGemini;
            _context = context;
            _postariAssistant = postariAssistant;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GenereazaSugestii(string prompt, string? oraseExcluse=null)
        {
            if (string.IsNullOrEmpty(prompt))
                return Json(new { success = false, message = "Prompt empty" });

            var raspunsAi = await _serviciuGemini.GenerareDestinatii(prompt, oraseExcluse);
            if (raspunsAi == null || raspunsAi.orase.Count == 0)
            {
                return Json(new { success = false, message = "AI Error or empty results" });
            }

            return Json(new
            {
                success = true,
                orase = raspunsAi.orase,
                descrieri = raspunsAi.descrieri,
                tags = raspunsAi.tags
            });
        }
        [HttpPost]
        public async Task<IActionResult> GetPostariAssistant([FromBody] PostariVibeRequest req)
        {
            if (req.Orase == null || req.Orase.Count == 0 || string.IsNullOrEmpty(req.Prompt))
                return Json(new { success = false, message = "Date invalide" });

            var postari = await _postariAssistant.GetPostariPentruVibe(req.Orase, req.Prompt);
            return Json(new { success = true, postari });
        }

        public class PostariVibeRequest
        {
            public List<string> Orase { get; set; } = new();
            public string Prompt { get; set; } = "";
        }
    }
}
