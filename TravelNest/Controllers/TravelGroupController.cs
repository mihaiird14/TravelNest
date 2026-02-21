using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Data.Migrations;
using TravelNest.Hubs;
using TravelNest.Models;
using TravelNest.Services;

namespace TravelNest.Controllers
{
    [Authorize]
    public class TravelGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<NotificariHub> _hubContext;
        public TravelGroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment env, IHubContext<NotificariHub> hubContext)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
            _hubContext = hubContext;
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
            var profilAdmin = await _context.Profils
                                    .Include(p => p.User)
                                    .FirstOrDefaultAsync(p => p.UserId == adminId);

            if (profilAdmin == null) return BadRequest();

            grup.AdminId = profilAdmin.Id;
            grup.ListaParticipanti = new List<MembruGrup>();
            grup.Locatii = new List<LocatieGrup>();

            if (imagineFisier != null && imagineFisier.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "trips");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

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
                    grup.ListaParticipanti.Add(new MembruGrup
                    {
                        ProfilId = id,
                        Confirmare = "PENDING",
                        DataInscrierii = DateTime.Now
                    });
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

            if (idPrieteni != null && idPrieteni.Any())
            {
                var notificariDeTrimis = new List<Notificare>();

                foreach (var idPrieten in idPrieteni)
                {
                    var notificare = new Notificare
                    {
                        destinatarId = idPrieten,
                        expeditorId = profilAdmin.Id,
                        TitluNotificare = "Travel Group Invite",
                        MesajNotificare = $"{profilAdmin.User.UserName} invited you to join {grup.Nume ?? "a travel group"}!",
                        TipNotificare = "TGInvite",
                        DataTrimitere = DateTime.Now,
                        EsteCitita = false
                    };
                    _context.Notificari.Add(notificare);
                    notificariDeTrimis.Add(notificare);
                }

                await _context.SaveChangesAsync();

                foreach (var notif in notificariDeTrimis)
                {
                    var profilDestinatar = await _context.Profils.FindAsync(notif.destinatarId);
                    if (profilDestinatar != null)
                    {
                        await _hubContext.Clients.User(profilDestinatar.UserId).SendAsync("PrimesteNotificare",
                            notif.TitluNotificare,
                            notif.MesajNotificare,
                            notif.TipNotificare,
                            profilAdmin.User.UserName,
                            notif.Id);
                    }
                }
            }

            return RedirectToAction("Vizualizare", new { id = grup.Id });
        }
        public async Task<IActionResult> Vizualizare(int id)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            var grup = await _context.TravelGroups
                .Include(x => x.Locatii)
                .Include(x => x.Documente)
                .Include(x => x.ListaParticipanti)
                    .ThenInclude(x => x.Profil)
                        .ThenInclude(pr => pr.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (grup == null)
            {
                return NotFound(); 
            }
            grup.ListaParticipanti = grup.ListaParticipanti
                .OrderBy(m => m.Confirmare == "ORGANIZER" ? 0 :
                             m.Confirmare == "MEMBER" ? 1 : 2)
                .ToList();
            bool esteMembru = grup.ListaParticipanti.Any(p => p.ProfilId == profil.Id && p.Confirmare == "MEMBER");
            bool esteAdmin = grup.AdminId == profil.Id;
            if (!esteMembru && !esteAdmin)
            {
                return RedirectToAction("Index", "Profil");
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
                var utilizatorConectat = await _userManager.GetUserAsync(User);
                var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
                bool esteMembru = grup.ListaParticipanti.Any(p => p.ProfilId == profil.Id);
                bool esteAdmin = grup.AdminId == profil.Id;
                if (!esteMembru && !esteAdmin)
                {
                    return RedirectToAction("Index", "Profil");
                }
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
        [HttpPost]
        public async Task<IActionResult> ActualizarePerioada(string id, DateOnly dataPlecare, DateOnly dataIntoarcere)
        {
            var grup = await _context.TravelGroups.FindAsync(int.Parse(id));
            if (grup == null)
                    return NotFound();
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            bool esteMembru = grup.ListaParticipanti.Any(p => p.ProfilId == profil.Id);
            bool esteAdmin = grup.AdminId == profil.Id;
            if (!esteMembru && !esteAdmin)
            {
                return RedirectToAction("Index", "Profil");
            }
            grup.DataPlecare = dataPlecare;
            grup.DataIntoarcere = dataIntoarcere;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> ModificareDestinatieTG(int id, List<string> oraseSelectate, string thumbnailLink)
        {
            var grup = await _context.TravelGroups
                .Include(g => g.Locatii)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grup == null)
            {
                return Json(new { success = false, message = "The Group can not be found!" });
            }
            if (grup.Locatii != null && grup.Locatii.Any())
            {
                _context.LocatieGrups.RemoveRange(grup.Locatii);
            }

            if (oraseSelectate != null && oraseSelectate.Any())
            {
                foreach (var oras in oraseSelectate)
                {
                    grup.Locatii.Add(new LocatieGrup 
                    {   
                        Locatie = oras, 
                        GroupId = id 
                    });
                }
                if (!string.IsNullOrEmpty(thumbnailLink))
                {
                    grup.Thumbnail = thumbnailLink;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> IncarcaDocument(int groupId, IFormFile fisier)
        {
            if (fisier == null || fisier.Length == 0)
                return Json(new { success = false, message = "Iinvalid." });
            string folderDoc = Path.Combine(_env.WebRootPath, "documenteGrup");
            if (!Directory.Exists(folderDoc))
                Directory.CreateDirectory(folderDoc);
            string numeUnic = Guid.NewGuid().ToString() + "_" + fisier.FileName;
            string caleSistem = Path.Combine(folderDoc, numeUnic);
            using (var flux = new FileStream(caleSistem, FileMode.Create))
            {
                await fisier.CopyToAsync(flux);
            }
            var nouDoc = new DocumenteTG
            {
                NumeFisier = fisier.FileName,
                CaleFisier = "/documenteGrup/" + numeUnic,
                GroupId = groupId
            };

            _context.Documents.Add(nouDoc);
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                id = nouDoc.Id,
                nume = nouDoc.NumeFisier
            });
        }

        [HttpPost]
        public async Task<IActionResult> StergeDocument(int id)
        {
            var doc = await _context.Documents.FindAsync(id);
            if (doc == null)
                return Json(new { success = false, message = "The Doc does not exist." });

            try
            {
                string caleFizica = Path.Combine(_env.WebRootPath, doc.CaleFisier.TrimStart('/'));
                if (System.IO.File.Exists(caleFizica))
                {
                    System.IO.File.Delete(caleFizica);
                }
                _context.Documents.Remove(doc);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> TrimiteRequestParticipareTG(int groupId, int idDestinatar)
        {
            var userIdExpeditor = _userManager.GetUserId(User);
            if (userIdExpeditor == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            var profilExpeditor = await _context.Profils
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userIdExpeditor);
            var profilDestinatar = await _context.Profils
                .FirstOrDefaultAsync(p => p.Id == idDestinatar);

            if (profilDestinatar == null)
            {
                return Json(new { success = false, message = "Recipient profile not found." });
            }
            var grup = await _context.TravelGroups.FindAsync(groupId);
            if (grup == null)
            {
                return Json(new { success = false, message = "Group not found." });
            }
            var membruExistent = await _context.MembruGrups
                .AnyAsync(m => m.TravelGroup.Id == groupId && m.ProfilId == idDestinatar);

            if (!membruExistent)
            {
                var membruNou = new MembruGrup
                {
                    TravelGroupId = groupId,
                    ProfilId = idDestinatar,
                    Confirmare = "PENDING",
                    DataInscrierii = DateTime.Now
                };
                _context.MembruGrups.Add(membruNou);
            }
            var notificare = new Notificare
            {
                destinatarId = idDestinatar, 
                expeditorId = profilExpeditor.Id,
                TitluNotificare = "Travel Group Invite",
                MesajNotificare = $"{profilExpeditor.User.UserName} invited you to join {grup.Nume ?? "a travel group"}!",
                TipNotificare = "TGInvite",
                DataTrimitere = DateTime.Now,
                EsteCitita = false
            };
            _context.Notificari.Add(notificare);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.User(profilDestinatar.UserId).SendAsync("PrimesteNotificare",
                notificare.TitluNotificare,
                notificare.MesajNotificare,
                notificare.TipNotificare,
                profilExpeditor.User.UserName,
                notificare.Id); 
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> SearchFriends(string username, int groupId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new List<object>());
            }
            var utilizatorConectat = _userManager.GetUserId(User);
            var membriiTG = await _context.MembruGrups
                .Where(m => m.TravelGroup.Id == groupId)
                .Select(m => m.ProfilId)
                .ToListAsync();
            var rezultate = await _context.Profils
                .Include(p => p.User)
                .Where(p => (p.User.UserName.Contains(username))
                            && p.UserId != utilizatorConectat 
                            && !membriiTG.Contains(p.Id))
                .Take(10)
                .Select(p => new
                {
                    id = p.Id,
                    userName = p.User.UserName
                })
                .ToListAsync();

            return Json(rezultate);
        }
    }

}
