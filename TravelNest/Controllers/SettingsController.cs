using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public SettingsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
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
            var profil = await _context.Profils.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profil == null)
            {
                profil = new Profil
                {
                    UserId = user.Id,
                    ImagineProfil = "/images/profilDefault.png",
                };
                _context.Profils.Add(profil);
                await _context.SaveChangesAsync();
            }
            Console.WriteLine("dssdsdsd");
            return View(profil);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfil(Profil p)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.Include(u => u.Profil).FirstOrDefaultAsync(u => u.Id == p.UserId);

                if (user != null)
                {
                    if(p.User.UserName is null || p.User.UserName == "")
                    {
                        ModelState.AddModelError("User.UserName", "The Username can't be null.");
                        return View("Index", p);
                    }
                    if (p.User.UserName.Length < 5)
                    {
                        ModelState.AddModelError("User.UserName", "The Username must contain at least 5 characters.");
                        return View("Index", p);
                    }
                    var existaUser = await _userManager.FindByNameAsync(p.User.UserName);
                    if (existaUser != null && existaUser.Id != p.UserId)
                    {
                        ModelState.AddModelError("User.UserName", "This username is already taken.");
                        return View("Index", p);
                    }
                    if (user.UserName != p.User.UserName)
                    {

                        user.UserName = p.User.UserName;
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            return View("Index", p);
                        }
                    }
                    else
                    {
                        user.Profil.Bio = p.Bio;
                        await _userManager.UpdateAsync(user);
                        return View("Index", p);
                    }
                }
                return View("Index", p);
            }
            return View("Index", p);
        }
    }
}
