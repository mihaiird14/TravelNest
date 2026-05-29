using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelNest.Controllers
{
    [Authorize]
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MapController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null) 
                return View(new List<string>());
            var grupuriVizibile = await _context.TravelGroups
                .Include(g => g.Locatii)
                .Include(g => g.ListaParticipanti)
                .Where(g => (g.AdminId == profil.Id ||
                             g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .ToListAsync();

            var hartiAscunse = await _context.HartiAscunse
                .Where(h => h.ProfilId == profil.Id)
                .Select(h => h.TravelGroupId)
                .ToListAsync();

            var coduri = grupuriVizibile
                .Where(g => !hartiAscunse.Contains(g.Id))
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToList();
            Console.WriteLine("=== DEBUG MAP ===");
            Console.WriteLine($"ProfilId: {profil.Id}");
            Console.WriteLine($"Grupuri vizibile ({grupuriVizibile.Count}):");
            foreach (var g in grupuriVizibile)
            {
                Console.WriteLine($"  Grup ID={g.Id}, Locatii={string.Join(",", g.Locatii.Select(l => l.CodTara))}");
            }
            Console.WriteLine($"Harti ascunse ({hartiAscunse.Count}): {string.Join(",", hartiAscunse)}");
            var dupafiltru = grupuriVizibile.Where(g => !hartiAscunse.Contains(g.Id)).ToList();
            Console.WriteLine($"Dupa filtru ({dupafiltru.Count}):");
            foreach (var g in dupafiltru)
            {
                Console.WriteLine($"  Grup ID={g.Id}");
            }
            Console.WriteLine($"Coduri finale: {string.Join(",", coduri)}");
            Console.WriteLine("=== END DEBUG ===");

            // Add manually added countries
            var manualCountries = await _context.TariVizitate
                .Where(t => t.ProfilId == profil.Id)
                .Select(t => t.CodTara.ToUpper())
                .Distinct()
                .ToListAsync();

            var allCountries = coduri.Union(manualCountries).Distinct().ToList();
            
            return View(allCountries);
        }

        [HttpGet]
        public async Task<IActionResult> IstoricTari()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null) 
                return Json(new List<string>());
            var grupuriVizibile = await _context.TravelGroups
                .Include(g => g.Locatii)
                .Include(g => g.ListaParticipanti)
                .Where(g => (g.AdminId == profil.Id ||
                             g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .ToListAsync();

            var hartiAscunse = await _context.HartiAscunse
                .Where(h => h.ProfilId == profil.Id)
                .Select(h => h.TravelGroupId)
                .ToListAsync();

            var coduri = grupuriVizibile
                .Where(g => !hartiAscunse.Contains(g.Id))
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToList();

            var uniqueCountries = coduri.Distinct().ToList();
            
            return Json(uniqueCountries);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCountry([FromBody] CountryToggleRequest request)
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null)
                return Json(new { success = false, message = "Profil not found" });

            var codTara = request.CountryCode.ToUpper();

            // Check if country exists in any travel group
            var existsInGroup = await _context.TravelGroups
                .Where(g => !_context.HartiAscunse.Any(h => h.ProfilId == profil.Id && h.TravelGroupId == g.Id) &&
                           (g.AdminId == profil.Id ||
                            g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .Include(g => g.Locatii)
                .SelectMany(g => g.Locatii)
                .AnyAsync(l => l.CodTara.ToUpper() == codTara);

            // Find or create manual entry
            var manualEntry = await _context.TariVizitate
                .FirstOrDefaultAsync(t => t.ProfilId == profil.Id && t.CodTara == codTara);

            if (request.IsAdding)
            {
                // Add country
                if (manualEntry == null)
                {
                    _context.TariVizitate.Add(new TaraVizitata
                    {
                        ProfilId = profil.Id,
                        CodTara = codTara,
                        AdaugatManual = true
                    });
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Country added" });
                }
            }
            else
            {
                // Remove country - only if it doesn't exist in any group
                if (existsInGroup)
                    return Json(new { success = false, message = "Cannot remove country that is part of a travel group" });

                if (manualEntry != null)
                {
                    _context.TariVizitate.Remove(manualEntry);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Country removed" });
                }
            }

            return Json(new { success = false, message = "Invalid operation" });
        }
    }

    public class CountryToggleRequest
    {
        public string CountryCode { get; set; }
        public bool IsAdding { get; set; }
    }
}