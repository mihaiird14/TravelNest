using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using TravelNest.Data;
using TravelNest.Data.Migrations;
using TravelNest.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

                        Console.WriteLine("RASPUNS PYTHON: " + responseJson);
                         //DEBUG
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("PYTHON A DAT EROARE!");
                            return RedirectToAction("Index");
                        }

                        if (string.IsNullOrWhiteSpace(responseJson))
                        {
                            Console.WriteLine("PYTHON A TRIMIS RASPUNS GOL!");
                            return RedirectToAction("Index");
                        }

                        var embeddingResponse = JsonSerializer.Deserialize<FaceEmbeddingResponse>(
                            responseJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }
                        );

                        if (embeddingResponse != null && embeddingResponse.FacesEmb != null && embeddingResponse.FacesEmb.Any())
                        {
                            var emb = embeddingResponse.FacesEmb[0];

                            var faceEmb = new FaceEmbeddings
                            {
                                FisierMediaId = media.Id,
                                PersonId = null,
                                Embedding = JsonSerializer.Serialize(emb)
                            };

                            _context.FaceEmbeddings.Add(faceEmb);
                            await _context.SaveChangesAsync();
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EROARE : " + ex.ToString());
                    }
                }
            }
            return RedirectToAction("Index");
        }

    }
}