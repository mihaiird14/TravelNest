using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;

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
            if (profil == null) return View(new List<string>());

            var coduri = await _context.TravelGroups
                .Where(g => g.AdminId == profil.Id ||
                            g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER"))
                .Include(g => g.Locatii)
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToListAsync();

            return View(coduri);
        }
        [HttpGet]
        public async Task<IActionResult> IstoricTari()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null) 
                return Json(new List<string>());

            var coduri = await _context.TravelGroups
                .Where(g => g.AdminId == profil.Id ||
                            g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER"))
                .Include(g => g.Locatii)
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToListAsync();

            return Json(coduri);
        }
    }
}