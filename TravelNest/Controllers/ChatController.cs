using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profilEu = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profilEu == null)
                return RedirectToAction("Index", "Profil");

            var mesajeRecente = await _context.Mesaje
                .Include(m => m.Expeditor).ThenInclude(u => u.User)
                .Include(m => m.Destinatar).ThenInclude(u => u.User)
                .Include(m => m.VizualizariMessages)
                .Where(m => m.TravelGroupId == null && (m.ExpeditorProfilId == profilEu.Id || m.DestinatarProfilId == profilEu.Id))
                .OrderByDescending(m => m.DataTrimite)
                .ToListAsync();

            var listaConversatii = mesajeRecente
                .GroupBy(m => m.ExpeditorProfilId == profilEu.Id ? m.DestinatarProfilId : m.ExpeditorProfilId)
                .Select(g => {
                    var ultimulMesaj = g.First();
                    var celalaltProfil = ultimulMesaj.ExpeditorProfilId == profilEu.Id ? ultimulMesaj.Destinatar : ultimulMesaj.Expeditor;

                    return new
                    {
                        ProfilId = g.Key,
                        Nume = celalaltProfil?.User.UserName ?? "User",
                        Imagine = celalaltProfil?.ImagineProfil ?? "/images/default.png",
                        UltimulMesaj = ultimulMesaj.ContinutContent,
                        Data = ultimulMesaj.DataTrimite.ToString("HH:mm"),
                        Necitite = g.Count(m => m.DestinatarProfilId == profilEu.Id && !m.VizualizariMessages.Any(v => v.ProfilId == profilEu.Id))
                    };
                }).ToList();

            ViewBag.ConversatiiRecente = listaConversatii;
            ViewBag.ProfilEuId = profilEu.Id;

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetIstoricGrup(int idGrup)
        {
            var mesaje = await _context.Mesaje
                .Where(m => m.TravelGroupId == idGrup)
                .OrderBy(m => m.DataTrimite)
                .Select(m => new {
                    id = m.Id,
                    expeditorId = m.ExpeditorProfilId,
                    text = m.ContinutContent,
                    ora = m.DataTrimite.ToString("HH:mm"),
                    userName = m.Expeditor.User.UserName, 
                    avatarUrl = m.Expeditor.ImagineProfil,
                    dataFull = m.DataTrimite
                })

                .ToListAsync();

            return Json(mesaje);
        }

        [HttpPost]
        public async Task<IActionResult> MarcheazaCititeGrup(int idGrup)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profil == null) return Unauthorized();

            var mesajeNevazute = await _context.Mesaje
                .Where(m => m.TravelGroupId == idGrup &&
                            !m.VizualizariMessages.Any(v => v.ProfilId == profil.Id))
                .ToListAsync();

            foreach (var msg in mesajeNevazute)
            {
                _context.VizualizareMesaje.Add(new VizualizareMesaj
                {
                    MesajId = msg.Id,
                    ProfilId = profil.Id,
                    DataSeen = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetTotalNecititePrivat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profilEu = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profilEu == null) return Json(new { count = 0 });

            var count = await _context.Mesaje
                .Where(m => m.DestinatarProfilId == profilEu.Id &&
                            m.TravelGroupId == null &&
                            !m.VizualizariMessages.Any(v => v.ProfilId == profilEu.Id))
                .GroupBy(m => m.ExpeditorProfilId)
                .CountAsync();

            return Json(new { count });
        }
        [HttpGet]
        public async Task<IActionResult> GetIstoricPrivat(int destinatarId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profilEu = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            var mesaje = await _context.Mesaje
                .Where(m => m.TravelGroupId == null &&
                           ((m.ExpeditorProfilId == profilEu.Id && m.DestinatarProfilId == destinatarId) ||
                            (m.ExpeditorProfilId == destinatarId && m.DestinatarProfilId == profilEu.Id)))
                .OrderBy(m => m.DataTrimite)
                .Select(m => new {
                    expeditorId = m.ExpeditorProfilId,
                    text = m.ContinutContent,
                    ora = m.DataTrimite.ToString("HH:mm"),
                    dataFull = m.DataTrimite
                })
                .ToListAsync();

            return Json(mesaje);
        }
        [HttpGet]
        public async Task<IActionResult> CautaUtilizatori(string un)
        {
            if (string.IsNullOrWhiteSpace(un))
                return Json(new List<object>());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var r = await _context.Profils
                .Include(p => p.User)
                .Where(p => p.UserId != userId && p.User.UserName.Contains(un))
                .Take(5)
                .Select(p => new {
                    id = p.Id,
                    nume = p.User.UserName,
                    imagine = string.IsNullOrEmpty(p.ImagineProfil) ? "/images/default.png" : p.ImagineProfil
                })
                .ToListAsync();

            return Json(r);
        }
        [HttpPost]
        public async Task<IActionResult> MarcheazaCititePrivat(int idExpeditor)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profilEu = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profilEu == null)
                return Unauthorized();

            var mesajeNevazute = await _context.Mesaje
                .Where(m => m.DestinatarProfilId == profilEu.Id &&
                            m.ExpeditorProfilId == idExpeditor &&
                            m.TravelGroupId == null &&
                            !m.VizualizariMessages.Any(v => v.ProfilId == profilEu.Id))
                .ToListAsync();

            foreach (var msg in mesajeNevazute)
            {
                _context.VizualizareMesaje.Add(new VizualizareMesaj
                {
                    MesajId = msg.Id,
                    ProfilId = profilEu.Id,
                    DataSeen = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetNumarNecitite(int idGrup)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profilEu = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profilEu == null)
                   return Json(new { count = 0 });
            var count = await _context.Mesaje
                .Where(m => m.TravelGroupId == idGrup &&
                            !m.VizualizariMessages.Any(v => v.ProfilId == profilEu.Id))
                .CountAsync();

            return Json(new { count });
        }
    }
}
