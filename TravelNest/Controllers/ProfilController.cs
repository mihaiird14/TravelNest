using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TravelNest.Data;
using TravelNest.Data.Migrations;
using TravelNest.Models;
using TravelNest.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelNest.Controllers
{
    public class ProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CalculFaceRec _faceService; //functie calcul embeddings
        public ProfilController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, CalculFaceRec faceService)
        {
            _userManager = userManager;
            _context = context;
            _faceService = faceService;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var profil = await _context.Profils.Include(p => p.User)
                                            .Include(p => p.Posts).ThenInclude(post => post.FisiereMedia).FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profil == null)
            {
                profil = new Models.Profil
                {
                    UserId = user.Id,
                    ImagineProfil = "/images/profilDefault.png",
                };
                _context.Profils.Add(profil);
                await _context.SaveChangesAsync();
            }
            return View(profil);
        }
        [HttpGet]
        public JsonResult CautareTag(string val)
        {
            var users = _context.Profils
                           .Where(p => p.User.UserName.ToLower().Contains(val))
                           .Select(p => new
                           {
                               Id = p.User.Id,
                               Name = p.User.UserName,
                               Poza = p.ImagineProfil
                           })
                           .Take(3)
                           .ToList();

            return Json(users);
        }
        public async Task<IActionResult> AddPostare(int profilId, List<IFormFile> FisiereMedia, string locatie, string descriere, string tagUseri)
        {
            var profil = await _context.Profils.FindAsync(profilId);
            if (profil == null)
                return NotFound("Profilul nu există!");

            Postare postare = new Postare
            {
                CreatorId = profilId,
                Locatie = locatie,
                Descriere = descriere,
                DataCr = DateTime.Now
            };
            if (!string.IsNullOrEmpty(tagUseri))
            {

                var listaNume = tagUseri.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var idsDeSalvat = await _context.Users
                    .Where(u => listaNume.Contains(u.UserName))
                    .Select(u => u.Id)
                    .ToListAsync();
                postare.UseriMentionati = idsDeSalvat;
            }
            _context.Postares.Add(postare);
            await _context.SaveChangesAsync();

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/upload");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in FisiereMedia)
            {
                string ext = Path.GetExtension(file.FileName).ToLower();

                Tip type;
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp")
                    type = Tip.Image;
                else if (ext == ".mp4" || ext == ".mov" || ext == ".avi")
                    type = Tip.Video;
                else
                    continue;

                string newFile = Guid.NewGuid() + ext;
                string fullPath = Path.Combine(uploadPath, newFile);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var media = new FisierMedia
                {
                    Url = "/upload/" + newFile,
                    fisier = type,
                    PostareId = postare.Id
                };

                _context.FisierMedias.Add(media);
                await _context.SaveChangesAsync();

                //python face embeddings
                if (type == Tip.Image)
                {
                    string CaleFisier = Path.Combine(uploadPath, newFile);
                    var body = new
                    {
                        image_path = CaleFisier.Replace("\\", "/")
                    };

                    using var http = new HttpClient();
                    string json = JsonSerializer.Serialize(body);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = null;

                    try
                    {
                        response = await http.PostAsync("http://localhost:5001/faceEmb", content);
                        var responseJson = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(responseJson))
                        {
                            Console.WriteLine("[EROARE] Python a returnat eroare sau raspuns gol.");
                            continue;
                        }
                        var embeddingResponse = JsonSerializer.Deserialize<FaceEmbeddingResponse>(
                            responseJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );
                        if (embeddingResponse != null && embeddingResponse.FacesEmb != null)
                        {
                            int nrFete = embeddingResponse.FacesEmb.Count;
                            var feteDejaGasite = await _context.FaceEmbeddings
                                .Where(f => f.PersonId == profilId)
                                .AsNoTracking()
                                .ToListAsync();

                            var allOtherFaces = await _context.FaceEmbeddings
                                .Where(f => f.PersonId != null && f.PersonId != profilId)
                                .AsNoTracking()
                                .ToListAsync();

                            foreach (var embVector in embeddingResponse.FacesEmb)
                            {
                                var faceEmb = new FaceEmbeddings
                                {
                                    FisierMediaId = media.Id,
                                    PersonId = null, 
                                    Embedding = JsonSerializer.Serialize(embVector)
                                };
                                bool gasit = false;
                                if (feteDejaGasite.Count > 0)
                                {
                                    double distMinimaUser = double.MaxValue;
                                    foreach (var known in feteDejaGasite)
                                    {
                                        var knownVector = JsonSerializer.Deserialize<List<double>>(known.Embedding);
                                        var dist = _faceService.CalculDistanta(embVector, knownVector);
                                        if (dist < distMinimaUser) distMinimaUser = dist;
                                    }

                                    if (distMinimaUser < 0.6)
                                    {
                                        faceEmb.PersonId = profilId;
                                        gasit = true;
                                  
                                    }
                                }
                                if (!gasit && nrFete == 1)
                                {
                                    if (profilId > 0)
                                    {
                                        faceEmb.PersonId = profilId;
                                        gasit = true;
                                    }
                                }
                                _context.FaceEmbeddings.Add(faceEmb);
                                await _context.SaveChangesAsync();
                                if (!gasit)
                                {
                                    double distMinimaGlobal = double.MaxValue;
                                    int? sugestieId = null;

                                    foreach (var other in allOtherFaces)
                                    {
                                        var otherVector = JsonSerializer.Deserialize<List<double>>(other.Embedding);
                                        var dist = _faceService.CalculDistanta(embVector, otherVector);
                                        if (dist < distMinimaGlobal)
                                        {
                                            distMinimaGlobal = dist;
                                            sugestieId = other.PersonId;
                                        }
                                    }

                                    if (distMinimaGlobal < 0.6 && sugestieId != null)
                                    {
                                        var sugestie = new SugestieTag
                                        {
                                            FaceEmbeddingId = faceEmb.Id,
                                            SuggestedPersonId = sugestieId.Value,
                                            Confidence = 1 - distMinimaGlobal
                                        };
                                        _context.SugestieTags.Add(sugestie);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EROARE CRITICA LA PROCESARE: " + ex.ToString());
                    }
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<JsonResult> CheckFacesInPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Nu s-a trimis niciun fișier." });

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/upload/temp");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            string ext = Path.GetExtension(file.FileName).ToLower();
            string tempFileName = Guid.NewGuid() + ext;
            string fullPath = Path.Combine(uploadPath, tempFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var foundUsers = new List<object>();

            try
            {
                //apel
                var body = new { image_path = fullPath.Replace("\\", "/") };

                using var http = new HttpClient();
                string json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync("http://localhost:5001/faceEmb", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var embeddingResponse = JsonSerializer.Deserialize<FaceEmbeddingResponse>(
                        responseJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (embeddingResponse != null && embeddingResponse.FacesEmb != null)
                    {
                        //cauta db
                        var knownFaces = await _context.FaceEmbeddings
                            .Where(f => f.PersonId != null)
                            .Include(f => f.Person).ThenInclude(p => p.User)
                            .ToListAsync();

                        foreach (var newFaceVector in embeddingResponse.FacesEmb)
                        {
                            double bestDist = 10.0;
                            int? bestMatchId = null;

                            foreach (var known in knownFaces)
                            {
                                var knownVector = JsonSerializer.Deserialize<List<double>>(known.Embedding);
                           
                                var dist = _faceService.CalculDistanta(newFaceVector, knownVector);

                                if (dist < 0.6 && dist < bestDist)
                                {
                                    bestDist = dist;
                                    bestMatchId = known.PersonId;
                                }
                            }

                            if (bestMatchId != null)
                            {
                               
                                if (!foundUsers.Any(u => (int)((dynamic)u).ProfilId == bestMatchId))
                                {
                                    var match = knownFaces.First(k => k.PersonId == bestMatchId);
                                    foundUsers.Add(new
                                    {
                                        ProfilId = match.PersonId,
                                        UserName = match.Person.User.UserName,
                                        Poza = match.Person.ImagineProfil
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EROARE CheckFacesInPhoto: " + ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            return Json(new { success = true, suggestions = foundUsers });
        }
    }
}