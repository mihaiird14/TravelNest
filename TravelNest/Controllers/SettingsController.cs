using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        public SettingsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
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
                 .ThenInclude(post => post.FisiereMedia) // Necesar pentru imagini/video
             .Include(p => p.Posts)
                 .ThenInclude(post => post.Likes) // Necesar pentru verificarea aDatLike
             .FirstOrDefaultAsync(p => p.UserId == user.Id);
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
            return View(profil);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfil(Profil p, IFormFile? ImagineProfil, bool ResetImage = false)
        {
            ModelState.Remove("User");
            ModelState.Remove("ImagineProfil");
            ModelState.Remove("FaceEmbeddings");
            foreach (var x in ModelState.Keys.Where(k => k.StartsWith("User.")).ToList())
            {
                ModelState.Remove(x);
            }
            if (ModelState.IsValid)
            {
                var user = await _context.Users.Include(u => u.Profil).FirstOrDefaultAsync(u => u.Id == p.UserId);

                if (user != null)
                {
                    if (user.Profil.Bio != p.Bio)
                    {
                        user.Profil.Bio = p.Bio;
                        await _context.SaveChangesAsync();
                    }

                    if (ImagineProfil != null && ImagineProfil.Length > 0)
                    {
                        var extensii = new[] { ".jpg", ".jpeg", ".png" };
                        var fisierExtensie = Path.GetExtension(ImagineProfil.FileName).ToLower();

                        if (!extensii.Contains(fisierExtensie))
                        {
                            ModelState.AddModelError("ImagineProfil", "Extensie invalidă (doar jpg, jpeg, png).");
                             return View("Index", user.Profil);
                        }

                        var uploadPath = Path.Combine(_env.WebRootPath, "pozeProfil");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                        var numeUnic = Guid.NewGuid().ToString() + "_" + ImagineProfil.FileName;
                        var caleCompleta = Path.Combine(uploadPath, numeUnic);

                        using (var stream = new FileStream(caleCompleta, FileMode.Create))
                        {
                            await ImagineProfil.CopyToAsync(stream);
                        }
                        user.Profil.ImagineProfil = "/pozeProfil/" + numeUnic;
                        await _context.SaveChangesAsync();
                    }
                    else if (ResetImage)
                    {
                        user.Profil.ImagineProfil = "/images/profilDefault.png";
                        await _context.SaveChangesAsync();
                    }
                    if (!string.IsNullOrEmpty(p.User?.UserName) && user.UserName != p.User.UserName)
                    {
                        if (p.User.UserName.Length < 5)
                        {
                            ModelState.AddModelError("User.UserName", "The username must have at least 5 characters!");
                            return View("Index", user.Profil);
                        }
                        var userExistent = await _userManager.FindByNameAsync(p.User.UserName);
                        if (userExistent != null)
                        {
                            ModelState.AddModelError("User.UserName", "This username is already used by another user!");
                            return View("Index", user.Profil);
                        }

                        user.UserName = p.User.UserName;
                        var result = await _userManager.UpdateAsync(user);

                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View("Index", user.Profil);
                        }
                    }
                    return RedirectToAction("Index");
                }
            }

            var userDb = await _context.Users.Include(u => u.Profil).FirstOrDefaultAsync(u => u.Id == p.UserId);
            return View("Index", userDb?.Profil ?? p);
        }
        [HttpPost]
        public async Task<IActionResult> makePrivateProfile(int profilId, bool status)
        {
            try
            {
                var profil = await _context.Profils.FindAsync(profilId);
                if (profil == null) return Json(new { success = false, message = "Profilul nu a fost găsit." });

                profil.isPrivate = status;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AllowAutoTag(int profilId, bool status)
        {
            var profil = await _context.Profils.FindAsync(profilId);
            if (profil == null)
            {
                return NotFound();
            }
            profil.autoTag = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> AllowManualTagSearch(int profilId, bool status)
        {
            var profil = await _context.Profils.FindAsync(profilId);
            if (profil == null)
            {
                return NotFound();
            }
            profil.manualTag = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> SchimbaParola(string parolaCurenta, string parolaNoua)
        {
            var utilizatorConectat = await _userManager.GetUserAsync(User);
            if (utilizatorConectat == null)
                return Json(new { success = false, message = "The users can not be found!" });

            if (parolaCurenta == parolaNoua)
            {
                return Json(new { success = false, message = "The new password must be diffrent from the current password!" });
            }
            var r = await _userManager.ChangePasswordAsync(utilizatorConectat, parolaCurenta, parolaNoua);

            if (r.Succeeded)
            {
                return Json(new { success = true });
            }
            var erori = string.Join("<br/>", r.Errors.Select(e => e.Description));

            return Json(new { success = false, message = erori });
        }
    }
}
