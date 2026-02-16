using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;
        public TravelGroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
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
        public async Task<IActionResult> AdaugaGrup(TravelGroup grup, string[] oraseSelectate, int[] idPrieteni, IFormFile imagineFisier)
        {
            var adminId = _userManager.GetUserId(User);
            var profilAdmin = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == adminId);
            if (profilAdmin == null) 
                return BadRequest();
            grup.AdminId = profilAdmin.Id;
            if (imagineFisier != null && imagineFisier.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "trips");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string numeFisier = Guid.NewGuid().ToString() + Path.GetExtension(imagineFisier.FileName);
                string caleSistem = Path.Combine(folderPath, numeFisier);

                using (var stream = new FileStream(caleSistem, FileMode.Create))
                {
                    await imagineFisier.CopyToAsync(stream);
                }
                grup.Thumbnail = "/images/trips/" + numeFisier;
            }
            grup.ListaParticipanti.Add(new MembruGrup
            {
                ProfilId = profilAdmin.Id,
                DataInscrierii = DateTime.Now,
                Confirmare = "ORGANIZER"
            });

            if (idPrieteni != null)
            {
                foreach (var id in idPrieteni)
                {
                    grup.ListaParticipanti.Add(new MembruGrup { ProfilId = id, Confirmare = "PENDING" });
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
            return RedirectToAction("Vizualizare", new { id = grup.Id });
        }
        public async Task<IActionResult> Vizualizare(int id)
        {
            var grup = await _context.TravelGroups
                .Include(x => x.Locatii) 
                .Include(x => x.ListaParticipanti)
                    .ThenInclude(x => x.Profil)
                        .ThenInclude(pr => pr.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (grup == null)
            {
                return NotFound(); 
            }
            return View("Vizualizare", grup);
        }
        [HttpPost]
        public async Task<IActionResult> ActualizareBannerGrup(string id, string nume, string descriere, IFormFile imagineFisier, string thumbnailLink)
        {
            try
            {
                var grup = await _context.TravelGroups.FindAsync(int.Parse(id));
                if (grup == null) 
                    return NotFound();
                grup.Nume = nume;
                grup.Descriere = descriere;
                if (imagineFisier != null && imagineFisier.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "trips");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string numeFisier = Guid.NewGuid().ToString() + Path.GetExtension(imagineFisier.FileName);
                    string caleSistem = Path.Combine(folderPath, numeFisier);

                    using (var x = new FileStream(caleSistem, FileMode.Create))
                    {
                        await imagineFisier.CopyToAsync(x);
                    }
                    grup.Thumbnail = "/images/trips/" + numeFisier;
                }
                else if (!string.IsNullOrEmpty(thumbnailLink))
                {
                    grup.Thumbnail = thumbnailLink;
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Eroare: {ex.Message}");
            }
        }
    }

}
