using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    public class ProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProfilController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var profil = await _context.Profils.Include(p=>p.User).FirstOrDefaultAsync(p => p.UserId == user.Id);
            if(profil == null)
            {
                profil = new Profil
                {
                    UserId = user.Id,
                    ImagineProfil = "/images/profilDefault.png",
                };
                _context.Profils.Add(profil);
                await _context.SaveChangesAsync();
            }
            return View(profil);
        }
    }
}
