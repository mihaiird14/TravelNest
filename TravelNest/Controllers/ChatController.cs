using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
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
        public async Task<IActionResult> GetNumarNecitite(int idGrup)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profil == null)
                return Json(new { count = 0 });

           
            var count = await _context.Mesaje
                .Where(m => m.TravelGroupId == idGrup &&
                            m.ExpeditorProfilId != profil.Id && 
                            !m.VizualizariMessages.Any(v => v.ProfilId == profil.Id))
                .CountAsync();

            return Json(new { count });
        }
    }
}
