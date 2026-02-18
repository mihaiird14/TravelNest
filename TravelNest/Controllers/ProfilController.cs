using Microsoft.AspNetCore.Authorization;
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
using TravelNest.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace TravelNest.Controllers
{
    [Authorize]
    public class ProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CalculFaceRec _faceService;
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
            var profil = await _context.Profils
                .Include(p => p.User)
                .Include(p => p.Posts) 
                    .ThenInclude(post => post.FisiereMedia) 
                .Include(p => p.MembruGrupuri) 
                    .ThenInclude(mg => mg.TravelGroup)
                        .ThenInclude(l=>l.Locatii)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
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
        public async Task<IActionResult> CautareTag(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return Json(new List<object>());
            }

            var users = await _context.Profils
                .Where(u => u.User.UserName.Contains(val) && u.manualTag==true)
                .Take(5)
                .Select(u => new
                {

                    userName = u.User.UserName,
                    poza = u.ImagineProfil ?? "/images/profilDefault.png"
                })
                .ToListAsync();

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
                                        if (dist < distMinimaUser) 
                                            distMinimaUser = dist;
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
                            .Where(f => f.PersonId != null && f.Person.autoTag==false)
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
        [HttpGet]
        public async Task<IActionResult> InfoPostari(int postId)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            var postare = await _context.Postares
                    .Include(p => p.Profil)
                        .ThenInclude(pr => pr.User)
                    .Include(p => p.FisiereMedia)
                    .Include(p => p.Likes)
                    .Include(p => p.Comentarii)
                         .ThenInclude(c => c.LikeComentariu)
                    .Include(p => p.Comentarii)
                        .ThenInclude(c => c.Profil)
                            .ThenInclude(u => u.User)
                    .Include(p => p.Comentarii)
                        .ThenInclude(c => c.Raspunsuri)
                            .ThenInclude(r => r.User).ThenInclude(u => u.User)
                    .Include(p => p.Comentarii)
                        .ThenInclude(c => c.Raspunsuri)
                            .ThenInclude(r => r.LikeReplyComentarii)
                    .FirstOrDefaultAsync(p => p.Id == postId);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            if (postare == null)
            {
                return NotFound();
            }
            var userIds = postare.UseriMentionati ?? new List<string>();

            var listaTaguri = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new {
                    username = u.UserName,
                    userImage = (u.Profil != null && !string.IsNullOrEmpty(u.Profil.ImagineProfil))
                                ? u.Profil.ImagineProfil
                                : "/images/profilDefault.png"
                })
                .ToListAsync();
            var rezultatPostare = new
            {
                id = postare.Id,
                descriere = postare.Descriere,
                locatie = postare.Locatie,
                taguri = listaTaguri,
                data = postare.DataCr.ToString("dd MMM yyyy"),
                esteAutorPostare = (profil != null && postare.Profil.Id == profil.Id),
                username = postare.Profil.User.UserName,
                userImage = !string.IsNullOrEmpty(postare.Profil.ImagineProfil)
                            ? postare.Profil.ImagineProfil
                            : "/images/profilDefault.png",
                nrLikeuri = postare.Likes.Count(),
                esteApreciata = utilizatorConectat != null && postare.Likes.Any(l => l.UserId == utilizatorConectat.Id),
                media = postare.FisiereMedia.Select(f => new
                {
                    url = f.Url,
                    tip = f.fisier.ToString()
                }).ToList(),
                totalComentarii = postare.Comentarii.Count() + postare.Comentarii.SelectMany(c => c.Raspunsuri).Count(),
                comentarii = postare.Comentarii
                    .OrderBy(c => c.DataCr)
                    .Select(c => new
                    {
                        id = c.Id,
                        username = c.Profil.User.UserName,
                        poza = c.Profil.ImagineProfil ?? "/images/profilDefault.png",
                        continut = c.Continut,
                        data = c.DataCr.ToString("dd MMM"),
                        esteEditat = c.ComentariuEditat,
                        AutorComentariu = utilizatorConectat != null && c.Profil.UserId == utilizatorConectat.Id,
                        nrRaspunsuri = c.Raspunsuri.Count(),
                        nrLikeuriComentariu = c.LikeComentariu.Count(),
                        esteApreciat = c.LikeComentariu.Any(l => l.ProfilId == profil.Id),
                        raspunsuri = c.Raspunsuri.OrderBy(r => r.DataPost).Select(r => new
                        {
                            id = r.Id,
                            username = r.User.User.UserName,
                            userImage = r.User.ImagineProfil ?? "/images/profilDefault.png",
                            mesaj = r.Mesaj,
                            data = r.DataPost.ToString("dd MMM"),
                            nrLikeuriReply = r.LikeReplyComentarii.Count(),
                            euAmDatLikeReply = r.LikeReplyComentarii.Any(l => l.ProfilId == profil.Id),
                            esteEditat = r.RaspunsEditat,
                            autorReply = utilizatorConectat != null && r.UserId == profil.Id
                        }).ToList()
                    }).ToList()

            };

            return Json(rezultatPostare);
        }
        [HttpPost]
        public async Task<IActionResult> AdaugaComentariu([FromBody] ComentariuInput input)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            var p = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (p == null) 
                return NotFound("Profil negăsit");
            var comm = new Comentariu
            {
                PostareId = input.PostareId,
                ProfilId = p.Id,
                Continut = input.Continut,
                DataCr = DateTime.UtcNow
            };

            _context.Comentarii.Add(comm);
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                id = comm.Id,
                username = user.UserName,
                poza = p.ImagineProfil ?? "/images/profilDefault.png",
                continut = comm.Continut,
                data = "ACUM"
            });
        }
        [HttpPost]
        public async Task<IActionResult> LikePostare(int postId)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            if (utilizator == null)
                return Unauthorized();

            var likeExistent = await _context.LikesPostari
                .FirstOrDefaultAsync(l => l.PostareId == postId && l.UserId == utilizator.Id);
            bool esteLikedAcum;

            if (likeExistent != null)
            {
                _context.LikesPostari.Remove(likeExistent);
                esteLikedAcum = false;
            }
            else
            {
                var likeNou = new LikesPostare { PostareId = postId, UserId = utilizator.Id };
                _context.LikesPostari.Add(likeNou);
                esteLikedAcum = true;
            }
            await _context.SaveChangesAsync();
            int numarActualizat = await _context.LikesPostari.CountAsync(l => l.PostareId == postId);
            return Json(new { success = true, liked = esteLikedAcum, nrLikeuri = numarActualizat });
        }
        [HttpPost]
        public async Task<IActionResult> AddComReply(int comentariuId, string mesaj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mesaj))
                {
                    return Json(new { success = false, message = "The message cannot be empty!" });
                }
                if (comentariuId <= 0)
                {
                    return Json(new { success = false, message = "Invalid comment ID!" });
                }
                var utilizator = await _userManager.GetUserAsync(User);
                if (utilizator == null)
                    return Unauthorized();
                var profil = await _context.Profils.FirstOrDefaultAsync(pr => pr.UserId == utilizator.Id);
                if (profil == null)
                {
                    return Json(new { success = false, message = "Profile not found!" });
                }
                var raspunsComentariu = new ReplyCom
                {
                    ComentariuId = comentariuId,
                    Mesaj = mesaj,
                    DataPost = DateTime.UtcNow,
                    UserId = profil.Id
                };
                _context.ReplyComs.Add(raspunsComentariu);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    username = utilizator.UserName,
                    userImage = profil.ImagineProfil ?? "/images/profilDefault.png",
                    mesaj = raspunsComentariu.Mesaj,
                    data = "ACUM"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> StergeComentariu(int id)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            if (utilizator == null)
                return Unauthorized();

            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            if (profil == null)
                return NotFound();
            var comm = await _context.Comentarii
                .Include(c => c.Raspunsuri)
                .Include(c => c.Postare)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comm == null)
                return Json(new { success = false, message = "Comment not found!" });
            bool autorCom = comm.ProfilId == profil.Id;
            //bool esteProprietarPostare = comm.Postare.CreatorId == profil.Id;

            if (!autorCom)
            {
                return Json(new { success = false, message = "You do not have permission to delete this comment!" });
            }
            try
            {
                if (comm.Raspunsuri != null && comm.Raspunsuri.Any())
                {
                    _context.ReplyComs.RemoveRange(comm.Raspunsuri);
                }
                _context.Comentarii.Remove(comm);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditComment(int id, string continutUpdated)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            if (utilizatorConectat == null)
                return Unauthorized();
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            if (profil == null)
                return NotFound();
            var comentariu = await _context.Comentarii.FirstOrDefaultAsync(c => c.Id == id);
            if (comentariu == null)
                return Json(new { success = false, message = "Comment not found!" });
            if (comentariu.ProfilId != profil.Id)
            {
                return Json(new { success = false, message = "You do not have permission to edit this comment!" });
            }
            try
            {
                comentariu.Continut = continutUpdated;
                comentariu.ComentariuEditat = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true, newContent = comentariu.Continut });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        //functie pentru like-uri comentarii
        [HttpPost]
        public async Task<IActionResult> AdaugaLikeComentariu(int comId)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            if (utilizatorConectat == null)
                return Unauthorized();
            if (profil == null)
                return NotFound();
            var likeExistent = await _context.LikeComentarii
                .FirstOrDefaultAsync(l => l.ComentariuId == comId && l.ProfilId == profil.Id);
            bool esteLikedAcum;
            if (likeExistent != null)
            {
                _context.LikeComentarii.Remove(likeExistent);
                esteLikedAcum = false;
            }
            else
            {
                var likeCom = new LikeComentariu
                {
                    ComentariuId = comId,
                    ProfilId = profil.Id
                };
                _context.LikeComentarii.Add(likeCom);
                esteLikedAcum = true;
            }
            await _context.SaveChangesAsync();
            int nrLikesComs = await _context.LikeComentarii.CountAsync(l => l.ComentariuId == comId);
            return Json(new { success = true, liked = esteLikedAcum, nrLikeuri = nrLikesComs });
        }
        //functie like reply comentarii
        [HttpPost]
        public async Task<IActionResult> AdaugaLikeReplyComentariu(int replyId)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            if (utilizatorConectat == null)
                return Unauthorized();
            if (profil == null)
                return NotFound();
            var likeExistent = await _context.LikeReplyComentarii
                .FirstOrDefaultAsync(l => l.ReplyId == replyId && l.ProfilId == profil.Id);
            bool esteLikedAcum;
            if (likeExistent != null)
            {
                _context.LikeReplyComentarii.Remove(likeExistent);
                esteLikedAcum = false;
            }
            else
            {
                var likeReplyCom = new LikeReplyComentarii
                {
                    ReplyId = replyId,
                    ProfilId = profil.Id
                };
                _context.LikeReplyComentarii.Add(likeReplyCom);
                esteLikedAcum = true;
            }
            await _context.SaveChangesAsync();
            int nrLikesReplyComs = await _context.LikeReplyComentarii.CountAsync(l => l.ReplyId == replyId);
            return Json(new { success = true, liked = esteLikedAcum, nrLikeuri = nrLikesReplyComs });
        }
        //functie editare reply
        [HttpPost]
        public async Task<IActionResult>EditReply(int ReplyId,string continut)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            var reply = await _context.ReplyComs.FindAsync(ReplyId);
            if (reply == null || profil == null || reply.UserId != profil.Id) 
                    return Unauthorized();
            reply.Mesaj = continut;
            reply.RaspunsEditat = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true, mesajNou = reply.Mesaj });
        }
        //functie sterge reply 
        [HttpPost]
        public async Task<IActionResult> StergeReply(int ReplyId)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            var reply = await _context.ReplyComs.FindAsync(ReplyId);
            if (reply == null || profil == null || reply.UserId != profil.Id) 
                return Unauthorized();
            _context.ReplyComs.Remove(reply);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        //functie sterge postare
        [HttpPost]
        public async Task<IActionResult>StergePostare(int postareId)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            var postare = await _context.Postares
                .Include(p => p.Comentarii)
                .Include(p => p.FisiereMedia)
                .FirstOrDefaultAsync(p => p.Id == postareId);
            if (profil == null || postare == null || postare.CreatorId != profil.Id)
            {
                return Json(new { success = false, message = "You dont have permission!" });
            }
            if (postare.Comentarii != null && postare.Comentarii.Any())
            {
                _context.Comentarii.RemoveRange(postare.Comentarii);
            }
            if (postare.FisiereMedia != null && postare.FisiereMedia.Any())
            {
                _context.FisierMedias.RemoveRange(postare.FisiereMedia);
            }
            _context.Postares.Remove(postare);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        //functie de arhivare postare
        public async Task<IActionResult>ArhivarePostare(int postareId)
        {
            var utilizator = await _userManager.GetUserAsync(User); 
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            var postare = await _context.Postares.FirstOrDefaultAsync(p => p.Id == postareId);
            if (profil == null || postare == null || postare.CreatorId != profil.Id)
            {
                return Json(new { success = false, message = "You dont have permission!" });
            }
            postare.Arhivata = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true});
        }
        [HttpPost]
        public async Task<IActionResult> DezarhivarePostare(int postareId)
        {
            var utilizator = await _userManager.GetUserAsync(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizator.Id);
            var postare = await _context.Postares.FirstOrDefaultAsync(p => p.Id == postareId);
            if (profil == null || postare == null || postare.CreatorId != profil.Id)
            {
                return Json(new { success = false, message = "You dont have permission!" });
            }
            postare.Arhivata = false;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        //functie pentru editare postare
        [HttpPost]
        public async Task<IActionResult> EditeazaPostare(int postareId, string locatie, string descriere, string tagUseri)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            if (utilizatorConectat == null) 
                return Unauthorized();
            var postare = await _context.Postares.FirstOrDefaultAsync(p => p.Id == postareId);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == utilizatorConectat.Id);
            if (postare == null || profil == null || postare.CreatorId != profil.Id)
            {
                return Json(new { success = false, message = "You dont have permission to edit!" });
            }
            postare.Locatie = locatie;
            postare.Descriere = descriere;
            if (!string.IsNullOrEmpty(tagUseri))
            {
                try
                {
                    var listaNume = JsonSerializer.Deserialize<List<string>>(tagUseri);
                    var idsDeSalvat = await _context.Users
                        .Where(u => listaNume.Contains(u.UserName))
                        .Select(u => u.Id)
                        .ToListAsync();

                    postare.UseriMentionati = idsDeSalvat;
                }
                catch (Exception)
                {
                    var listaNumeManual = tagUseri.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var idsManual = await _context.Users
                        .Where(u => listaNumeManual.Contains(u.UserName))
                        .Select(u => u.Id)
                        .ToListAsync();
                    postare.UseriMentionati = idsManual;
                }
            }
            else
            {
                postare.UseriMentionati = new List<string>();
            }
            try
            {
                _context.Postares.Update(postare);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { openPostId = postareId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}