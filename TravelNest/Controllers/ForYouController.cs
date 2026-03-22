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
        public ForYouController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userProfil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (userProfil == null) 
                return NotFound();
            ViewBag.UserLog = userId;
            var urmaritiIds = await _context.Follows
                .Where(f => f.FollowerId == userProfil.Id && f.Status == StatusUrmarire.Accepted)
                .Select(f => f.FollowedId)
                .ToListAsync();

            var vazuteIds = await _context.VizualizarePostares
                .Where(v => v.VisitorProfilId == userProfil.Id)
                .Select(v => v.PostareId)
                .ToListAsync();
            IQueryable<Postare> queryBaza = _context.Postares
                .Include(p => p.FisiereMedia)
                .Include(p => p.Profil).ThenInclude(pr => pr.User)
                .Include(p => p.Likes) 
                .Include(p => p.Comentarii)
                       .ThenInclude(c => c.Raspunsuri)
                .Where(p => !p.Arhivata);
            var postariPrieteni = queryBaza
                    .Where(p => urmaritiIds.Contains(p.CreatorId))
                    .AsEnumerable()
                    .OrderByDescending(p => !vazuteIds.Contains(p.Id))
                    .ThenByDescending(p => p.DataCr)
                    .Take(21)
                    .ToList();
            //selectam metadatele de la postari la care userul a dat like
            var intereseLike = await _context.LikesPostari
                .Where(l => l.UserId == userId)
                .Select(l => l.Postare.MetaDate)
                .ToListAsync();
            //si metadate de la propriile postari
            var intereseProprii = await _context.Postares
                .Where(p => p.CreatorId == userProfil.Id)
                .Select(p => p.MetaDate)
                .ToListAsync();
            var interese = intereseLike.Concat(intereseProprii).ToList();
            var topTags = interese
                .Where(t => !string.IsNullOrEmpty(t))
                .SelectMany(t => t.Split(", "))
                .GroupBy(tag => tag)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var queryRecomandari = _context.Postares
                .Include(p => p.FisiereMedia)
                .Include(p => p.Profil).ThenInclude(pr => pr.User)
                .Where(p => !urmaritiIds.Contains(p.CreatorId) &&
                            p.CreatorId != userProfil.Id &&
                            !p.Arhivata &&
                            !vazuteIds.Contains(p.Id));

            List<Postare> postariNoi;
            if (topTags.Any())
            {
                postariNoi = queryRecomandari.AsEnumerable()
                    .Where(p => !string.IsNullOrEmpty(p.MetaDate) &&
                                topTags.Any(tag => p.MetaDate.Contains(tag)))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(9)
                    .ToList();
            }
            else
            {
                postariNoi = await queryRecomandari.OrderByDescending(p => p.DataCr).Take(9).ToListAsync();
            }
            //afisare amestecat
            var feedFinal = postariPrieteni.Concat(postariNoi)
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            return View(feedFinal);
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
