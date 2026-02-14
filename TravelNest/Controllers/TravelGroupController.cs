using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;
using TravelNest.Services;

namespace TravelNest.Controllers
{
    public class TravelGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public TravelGroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CautaPrieteniTravelGroup(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 2)
                return Json(new List<object>());
            var admin = _userManager.GetUserId(User);
            var prieteni = await _userManager.Users
                        .Where(u => u.Id != admin && u.UserName.Contains(username))
                        .Select(u => new {
                            id = u.Profil != null ? u.Profil.Id : 0,
                            userName = u.UserName,
                            pozaProfil = u.Profil != null && !string.IsNullOrEmpty(u.Profil.ImagineProfil)
                                         ? u.Profil.ImagineProfil
                                         : "/images/profilDefault.png"
                        })
                        .Where(x => x.id != 0) 
                        .Take(5)
                        .ToListAsync();

            return Json(prieteni);
        }
        [HttpPost]
        public async Task<IActionResult> AdaugaGrup(TravelGroup grup, string[] oraseSelectate, int[] idPrieteni)
        {
            var adminId = _userManager.GetUserId(User);
            var profilAdmin = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == adminId);
            if (profilAdmin == null) 
                return BadRequest();
            grup.AdminId = profilAdmin.Id;
            grup.ListaParticipanti.Add(new MembruGrup { ProfilId = profilAdmin.Id, DataInscrierii = DateTime.Now });
            if (idPrieteni != null)
            {
                foreach (var id in idPrieteni)
                {
                    grup.ListaParticipanti.Add(new MembruGrup { ProfilId = id });
                }
            }
            if (oraseSelectate != null)
            {
                foreach (var loc in oraseSelectate)
                {
                    grup.Locatii.Add(new LocatieGrup { Locatie = loc });
                }
            }
            _context.TravelGroups.Add(grup);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
