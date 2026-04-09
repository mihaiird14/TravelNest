using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Net.NetworkInformation;
using TravelNest.Data;
using TravelNest.Data.Migrations;
using TravelNest.Hubs;
using TravelNest.Models;
using TravelNest.Services;
using TravelNest.ViewModels;
using QuestPDF.Helpers;
using System.Net.Http;
namespace TravelNest.Controllers
{
    [Authorize]
    public class TravelGroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<NotificariHub> _hubContext;
        private readonly FlightService _flightService;
        public TravelGroupController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment env, IHubContext<NotificariHub> hubContext, FlightService flightService)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
            _hubContext = hubContext;
            _flightService = flightService;
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
                .Include(x=>x.Zboruri)
                .Include(x => x.ListaParticipanti)
                    .ThenInclude(x => x.Profil)
                        .ThenInclude(pr => pr.User)
                .FirstOrDefaultAsync(x => x.Id == id);
            ViewBag.CurrentProfilId = profil?.Id;
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
                return Json(new { success = false, message = "Profile not found." });
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
        [HttpPost]
        public async Task<IActionResult> StergeMembru(int groupId, int profilIdDeEliminat)
        {
            var userConectat = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userConectat);
            var grup = await _context.TravelGroups
                .Include(g => g.ListaParticipanti)
                .Include(g => g.Locatii) 
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (grup == null || profil == null)
                return Json(new { success = false, message = "Data error" });

            bool admin = grup.AdminId == profil.Id;
            bool stergeSelf = profil.Id == profilIdDeEliminat;

            if (!admin && !stergeSelf)
                return Json(new { success = false, message = "Permission denied" });

            var membruDeEliminat = grup.ListaParticipanti.FirstOrDefault(m => m.ProfilId == profilIdDeEliminat);
            if (membruDeEliminat == null)
            {
                return Json(new { success = false, message = "Member not found" });
            }
            if (membruDeEliminat.Confirmare == "ORGANIZER")
            {
                var nextOrganizer = grup.ListaParticipanti
                    .Where(m => m.ProfilId != profilIdDeEliminat && m.Confirmare == "MEMBER")
                    .OrderBy(m => m.DataInscrierii)
                    .FirstOrDefault();

                if (nextOrganizer != null)
                {
                    nextOrganizer.Confirmare = "ORGANIZER";
                    grup.AdminId = nextOrganizer.ProfilId;
                }
                else
                {
                    if (grup.Locatii.Any())
                    {
                        _context.LocatieGrups.RemoveRange(grup.Locatii);
                    }
                    if (grup.ListaParticipanti.Any())
                    {
                        _context.MembruGrups.RemoveRange(grup.ListaParticipanti);
                    }
                    _context.TravelGroups.Remove(grup);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, redirected = true, url = Url.Action("Index", "Profil") });
                }
            }

            _context.MembruGrups.Remove(membruDeEliminat);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirected = stergeSelf, url = Url.Action("Index", "Profil") });
        }
        [HttpGet]
        public async Task<IActionResult> GetLocatiiTG(int idGrup)
        {
            var orase = await _context.LocatieGrups
                .Where(loc => loc.GroupId == idGrup)
                .OrderBy(loc => loc.Id) 
                .Select(loc => loc.Locatie)
                .ToListAsync();

            return Json(orase);
        }

        [HttpPost]
        public async Task<IActionResult> CautaZboruri([FromBody] List<CerereZbor> rute)
        {
            var rezultateFinale = new List<object>();

            foreach (var ruta in rute)
            {
                var zboruriUnice = new Dictionary<string, ZborGrupuri>();
                var tAmadeus = _flightService.SearchFlights(ruta.IataDeLa, ruta.IataLa, ruta.DataZbor);
                var tKiwi = _flightService.cautaKiwi(ruta.IataDeLa, ruta.IataLa, ruta.DataZbor, ruta.IdGrup);
               try
                {
                    await Task.WhenAll(tAmadeus, tKiwi);
                }
                catch { }
                if (tAmadeus.Status == TaskStatus.RanToCompletion)
                {
                    var lAmadeus = _flightService.cautaAmadeus(tAmadeus.Result, ruta.IdGrup, ruta.DeLa, ruta.La);
                    foreach (var z in lAmadeus)
                    {
                        string cheie = $"{z.NumeCompanie}{z.NumarZbor}_{z.DataPlecare:yyyyMMddHHmm}";
                        zboruriUnice[cheie] = z;
                    }
                }
                /*if (tKiwi.Status == TaskStatus.RanToCompletion)
                {
                    var kiwiList = tKiwi.Result;
                    foreach (var z in kiwiList)
                    {
                        string cheie = $"{z.NumeCompanie}{z.NumarZbor}_{z.DataPlecare:yyyyMMddHHmm}";
                        if (zboruriUnice.ContainsKey(cheie))
                        {
                            if (z.Pret < zboruriUnice[cheie].Pret) zboruriUnice[cheie] = z;
                        }
                        else
                        {
                            zboruriUnice.Add(cheie, z);
                        }
                    }
                }*/

                rezultateFinale.Add(new
                {
                    titluRuta = $"{ruta.DeLa} - {ruta.La}",
                    zboruri = zboruriUnice.Values.OrderBy(x => x.Pret).ToList()
                });
            }

            return Ok(rezultateFinale);
        }
        [HttpPost]
        public async Task<IActionResult> SalveazaZboruriTG([FromBody] List<ZborGrupuri> biletePrimite)
        {
            if (biletePrimite == null || !biletePrimite.Any())
            {
                return BadRequest(new { eroare = "No tickets received." });
            }
            int idGrup = biletePrimite.First().GrupId;
            using var tranzactie = await _context.Database.BeginTransactionAsync();
            try
            {
                //sterg zborurile vechi ca sa pot sa editez
                //dupa adaug
                var zboruriExistente = _context.ZborGrupuris.Where(z => z.GrupId == idGrup);
                _context.ZborGrupuris.RemoveRange(zboruriExistente);
                _context.ZborGrupuris.AddRange(biletePrimite);
                await _context.SaveChangesAsync();
                await tranzactie.CommitAsync();

                return Ok(new { succes = true, mesaj = "Tickets have been updated!" });
            }
            catch (Exception ex)
            {          
                await tranzactie.RollbackAsync();
                return BadRequest(new { eroare = "Database error: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetZboruriGrup(int idGrup)
        {
            try
            {
                var zboruri = await _context.ZborGrupuris
                    .Where(z => z.GrupId == idGrup)
                    .OrderBy(z => z.DataPlecare)
                    .ToListAsync();

                return Ok(zboruri);
            }
            catch (Exception ex)
            {
                return BadRequest(new { eroare = ex.Message });
            }
        }
        private string LinkZbor(ZborGrupuri zbor)
        {
            if (!string.IsNullOrEmpty(zbor.LinkZbor))
            {
                return zbor.LinkZbor;
            }
            string query = Uri.EscapeDataString($"Flights from {zbor.AeroportPlecare} to {zbor.AeroportSosire} on {zbor.DataPlecare:yyyy-MM-dd} {zbor.NumeCompanie}");
            return $"https://www.google.com/travel/flights?q={query}";

        }
        [HttpGet]
        public async Task<IActionResult> GenerarePDFZboruri(int idGrup, bool previzualizare = false)
        {
            var zboruri = await _context.ZborGrupuris
                .Where(z => z.GrupId == idGrup)
                .OrderBy(z => z.DataPlecare)
                .ToListAsync();

            if (zboruri == null || !zboruri.Any())
                return NotFound();
            var logoZbor = new Dictionary<int, byte[]>();
            using (var cl = new HttpClient())
            {
                cl.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; TravelNest/1.0)");

                foreach (var z in zboruri)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(z.Logo))
                        {
                            byte[] imageBytes = await cl.GetByteArrayAsync(z.Logo);
                            logoZbor[z.Id] = imageBytes;
                        }
                    }
                    catch
                    {
                        logoZbor[z.Id] = null;
                    }
                }
            }
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));
                    page.Header().PaddingBottom(20).Row(r =>
                    {
                        r.RelativeItem().Column(x =>
                        {
                            x.Item().Text("TRAVEL NEST").FontSize(28).ExtraBold().FontColor("#EE5607");
                            x.Item().Text("Official Trip Flights").FontSize(12).Italic().FontColor(Colors.Grey.Medium);
                        });
                        string linkLogo = Path.Combine(_env.WebRootPath, "images", "logo.png");
                        if (System.IO.File.Exists(linkLogo))
                        {
                            r.ConstantItem(150).Image(linkLogo);
                        }
                    });
                    page.Content().Column(column =>
                    {
                        foreach (var z in zboruri)
                        {
                            column.Item().PaddingBottom(25).Container()
                                .Border(1).BorderColor(Colors.Grey.Lighten3)
                                .Background(Colors.Grey.Lighten5)
                                .Padding(20)
                                .Column(flightCol =>
                                {
                                    flightCol.Item().Row(r =>
                                    {
                                        if (logoZbor.ContainsKey(z.Id) && logoZbor[z.Id] != null)
                                            r.ConstantItem(45).Image(logoZbor[z.Id]);
                                        else
                                            r.ConstantItem(45).Placeholder();

                                        r.RelativeItem().PaddingLeft(15).Column(c => {
                                            c.Item().Text($"{z.NumeCompanie} • Flight {z.NumarZbor}").Bold().FontSize(13);
                                            /*c.Item().Hyperlink(LinkZbor(z))
                                                    .Text("Book on Google FLights →")
                                                    .FontColor("#EE5607").Underline().FontSize(10);*/
                                        });
                                        r.ConstantItem(120).AlignRight().Column(c => {
                                            c.Item().Text("Total Price").FontSize(9).AlignRight();
                                            c.Item().Text($"{z.Pret} EUR").FontSize(18).ExtraBold().FontColor("#EE5607");
                                        });
                                    });
                                    flightCol.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                    flightCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Column(c => {
                                            c.Item().Text(z.OrasPlecare).ExtraBold().FontSize(15);
                                            c.Item().Text(z.AeroportPlecare).Bold().FontColor(Colors.Grey.Medium);
                                            c.Item().PaddingTop(5).Text(z.DataPlecare.ToString("HH:mm")).FontSize(14).Bold();
                                            c.Item().Text(z.DataPlecare.ToString("ddd, dd MMM yyyy")).FontSize(9);
                                        });

                                        row.RelativeItem(1.2f).AlignCenter().Column(c => {
                                            c.Item().AlignCenter().Text("----------------------").FontColor(Colors.Grey.Lighten1);
                                        });
                                        row.RelativeItem().AlignRight().Column(c => {
                                            c.Item().Text(z.OrasSosire).ExtraBold().FontSize(15);
                                            c.Item().Text(z.AeroportSosire).Bold().FontColor(Colors.Grey.Medium);
                                            c.Item().PaddingTop(5).Text(z.DataSosire.ToString("HH:mm")).FontSize(14).Bold();
                                            c.Item().Text(z.DataSosire.ToString("ddd, dd MMM yyyy")).FontSize(9);
                                        });
                                    });
                                });
                        }
                    });
                    page.Footer().PaddingTop(20).AlignCenter().Text(x => {
                        x.Span("Generated by ").FontSize(10);
                        x.Span("TravelNest").Bold().FontColor("#EE5607").FontSize(10);
                        x.Span($" on {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);
                    });
                });
            });
            var pdfBytes = document.GeneratePdf();

            if (previzualizare)
                return File(pdfBytes, "application/pdf");

            return File(pdfBytes, "application/pdf", $"Itinerariu_TravelNest_{idGrup}.pdf");
        }
        [HttpPost]
        public async Task<IActionResult> CreareAutomata([FromBody] List<string> orase)
        {
            if (orase == null || !orase.Any()) 
                return BadRequest();

            var userId = _userManager.GetUserId(User);
            var profilAdmin = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);

            string thumbnail = "/images/default-trip.jpg";
            string primulOras = orase[0];

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (TravelNest/1.0)");
                    string url = $"https://en.wikipedia.org/w/api.php?action=query&titles={Uri.EscapeDataString(primulOras)}&prop=pageimages&format=json&pithumbsize=1000";
                    var resp = await client.GetStringAsync(url);
                    using var doc = System.Text.Json.JsonDocument.Parse(resp);
                    var pages = doc.RootElement.GetProperty("query").GetProperty("pages");
                    foreach (var page in pages.EnumerateObject())
                    {
                        if (page.Value.TryGetProperty("thumbnail", out var t))
                            thumbnail = t.GetProperty("source").GetString();
                    }
                }
                catch { }
            }

            var grup = new TravelGroup
            {
                Nume = "Trip to " + primulOras,
                Descriere = "Automatic plan from Travel Assistant.",
                AdminId = profilAdmin.Id,
                DataPlecare = null, 
                DataIntoarcere = null,
                Thumbnail = thumbnail,
                Locatii = orase.Select(loc => new LocatieGrup { Locatie = loc }).ToList(),
                ListaParticipanti = new List<MembruGrup> {
            new MembruGrup { ProfilId = profilAdmin.Id, Confirmare = "ORGANIZER", DataInscrierii = DateTime.Now }
        }
            };

            _context.TravelGroups.Add(grup);
            await _context.SaveChangesAsync();
            return Json(new { success = true, groupId = grup.Id });
        }
    }

}
