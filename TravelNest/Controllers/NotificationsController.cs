using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using TravelNest.Data;
using TravelNest.Models;
namespace TravelNest.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var p = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var notificari = await _context.Notificari
                            .Include(n => n.Expeditor)
                            .ThenInclude(e => e.User)
                            .Where(n => n.destinatarId == user.Profil.Id)
                            .OrderByDescending(n => n.DataTrimitere)
                            .ToListAsync();

            return View(notificari);
        }
        [HttpPost]
        public async Task<IActionResult> RaspundeInviteTG(int notificareId, bool accepta)
        {
            var notificare = await _context.Notificari.FindAsync(notificareId);
            if (notificare == null)
            {
                return Json(new { success = false, message = "The notification was not found!" });
            }
            var membru = await _context.MembruGrups
                .FirstOrDefaultAsync(m => m.ProfilId == notificare.destinatarId && m.Confirmare == "PENDING");

            if (membru != null)
            {
                if (accepta)
                {
                    membru.Confirmare = "MEMBER";
                }
                else
                {
                    _context.MembruGrups.Remove(membru);
                }
            }
            _context.Notificari.Remove(notificare);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> NrNotificariNecitite()
        {
            var utilizator = await _userManager.GetUserAsync(User);
            if (utilizator==null)
            {
                return Json(new { count = 0 });
            }
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            if (profil == null)
            {
                return Json(new { count = 0 });
            }
            var numar = await _context.Notificari
                .CountAsync(n => n.destinatarId == profil.Id && !n.EsteCitita);
            return Json(new { count = numar }); 
        }
    }
}
