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
        public IActionResult Index()
        {
            return View();
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
