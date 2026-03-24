using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    [Authorize]
    public class ForYouController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GrafInterfata _grafService;
        public ForYouController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, GrafInterfata grafService)
        {
            _userManager = userManager;
            _context = context;
            _grafService = grafService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userProfil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userProfil == null) 
                return NotFound();

            ViewBag.UserLog = userId;
            //add in grafdb
            await _grafService.SincronizareUtilizator(userProfil.Id);
            var profiluriSugerateIds = await _grafService.ConturiSugerate(userProfil.Id);
            var conturiSugerate = await _context.Profils
                .Include(p => p.User)
                .Where(p => profiluriSugerateIds.Contains(p.Id))
                .ToListAsync();
            ViewBag.ConturiSugerate = conturiSugerate.OrderBy(x => profiluriSugerateIds.IndexOf(x.Id)).ToList();
            var urmaritiIds = await _context.Follows
                .Where(f => f.FollowerId == userProfil.Id && f.Status == StatusUrmarire.Accepted)
                .Select(f => f.FollowedId).ToListAsync();

            var vazuteIds = await _context.VizualizarePostares
                .Where(v => v.VisitorProfilId == userProfil.Id)
                .Select(v => v.PostareId).ToListAsync();

            // postari de la useri pe care ii urmaresc
            var postariFL = await _context.Postares
                .Include(p => p.FisiereMedia)
                .Include(p => p.Profil).ThenInclude(pr => pr.User)
                .Include(p => p.Likes)
                .Include(p => p.Comentarii)
                    .ThenInclude(c => c.Raspunsuri)
                .Where(p => urmaritiIds.Contains(p.CreatorId) && !p.Arhivata)
                .ToListAsync();

            // sortare, daca a fost vazuta sau nu si dupa data
            postariFL = postariFL
                .OrderByDescending(p => !vazuteIds.Contains(p.Id))
                .ThenByDescending(p => p.DataCr)
                .Take(21).ToList();

            //metaDate
            var intereseLike = await _context.LikesPostari.Where(l => l.UserId == userId).Select(l => l.Postare.MetaDate).ToListAsync();
            var intereseProprii = await _context.Postares.Where(p => p.CreatorId == userProfil.Id).Select(p => p.MetaDate).ToListAsync();
            var topTags = intereseLike.Concat(intereseProprii)
                .Where(t => !string.IsNullOrEmpty(t))
                .SelectMany(t => t.Split(", "))
                .GroupBy(tag => tag)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key).
                Take(3)
                .ToList();
            //postari recomandare dupa metaTAgs
            //doar conturi publice
            IQueryable<Postare> queryRecomandari = _context.Postares
                .Include(p => p.FisiereMedia)
                .Include(p => p.Profil)
                    .ThenInclude(pr => pr.User)
                .Include(p => p.Likes)        
                .Include(p => p.Comentarii)
                .Where(p => !urmaritiIds.Contains(p.CreatorId) &&
                            p.CreatorId != userProfil.Id &&
                            !p.Arhivata &&
                            !p.Profil.isPrivate);
            var postariNoi = await ObtinePostariSugerate(queryRecomandari, topTags, vazuteIds, true);
            //daca nu exista postari pe fyp, se elimina restrictia de vizualizare
            if (!postariFL.Any() && !postariNoi.Any())
            {
                postariNoi = await ObtinePostariSugerate(queryRecomandari, topTags, vazuteIds, false);
            }

            var feedFinal = postariFL.Concat(postariNoi).OrderBy(x => Guid.NewGuid()).ToList();
            return View(feedFinal);
        }
        private async Task<List<Postare>> ObtinePostariSugerate(IQueryable<Postare> query, List<string> tags, List<int> vazute, bool doarNevazute)
        {
            var q = query;
            if (doarNevazute)
            {
                q = q.Where(p => !vazute.Contains(p.Id));
            }

            if (tags.Any())
            {
                return q.AsEnumerable()
                    .Where(p => !string.IsNullOrEmpty(p.MetaDate) && tags.Any(tag => p.MetaDate.Contains(tag)))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(9)
                    .ToList();
            }

            return await q.OrderByDescending(p => p.DataCr).Take(9).ToListAsync();
        }
        [HttpGet]
        public async Task<IActionResult> CautaUtilizatori(string un)
        {
            if (string.IsNullOrEmpty(un) || un.Length < 3)
                return BadRequest();
            var idUtilizatorConectat = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var useri = await _context.Profils
                .Where(u => u.User.UserName.Contains(un) && u.User.Id != idUtilizatorConectat)
                .Select(u => new {
                    u.Id,
                    u.User.UserName,
                    u.ImagineProfil
                })
                .Take(10)
                .ToListAsync();

            return Json(useri);
        }

    }
}
