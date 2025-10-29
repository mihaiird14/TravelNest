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
        public async Task<IActionResult> EditProfil(Profil p, IFormFile? ImagineProfil, bool ResetImage = false)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.Include(u => u.Profil).FirstOrDefaultAsync(u => u.Id == p.UserId);

                if (user != null)
                {
                    user.Profil.Bio = p.Bio;
                    await _userManager.UpdateAsync(user);
                    if (ImagineProfil != null && ImagineProfil.Length > 0)
                    {
                        var extensii = new[] { ".jpg", ".jpeg", ".png" };
                        var fisierExtensie = Path.GetExtension(ImagineProfil.FileName).ToLower();
                        if (!extensii.Contains(fisierExtensie))
                        {
                            ModelState.AddModelError("ImagineProfil", "The file must be a picture with one of these extensions: (jpg, jpeg, png).");
                            return View("Index", user.Profil);
                        }
                        var locatieImg = Path.Combine(_env.WebRootPath, "pozeProfil", ImagineProfil.FileName);
                        var fisierBD = "/pozeProfil/" + ImagineProfil.FileName;
                        using (var fileStream = new FileStream(locatieImg, FileMode.Create))
                        {
                            await ImagineProfil.CopyToAsync(fileStream);
                        }
                        ModelState.Remove(nameof(p.ImagineProfil));

                        if (TryValidateModel(p))
                        {
                            user.Profil.ImagineProfil = fisierBD;
                            await _context.SaveChangesAsync();

                        }
                    }
                    else if(ResetImage)
                    {
                        user.Profil.ImagineProfil = "images/profilDefault.png";
                        await _userManager.UpdateAsync(user);
                        return RedirectToAction("Index");
                    }
                    if (p.User.UserName is null || p.User.UserName == "")
                    {
                        ModelState.AddModelError("User.UserName", "The Username can't be null.");
                        return View("Index", user.Profil);
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
                        return View("Index", user.Profil);
                    }
                    if (user.UserName != p.User.UserName)
                    {

                        user.UserName = p.User.UserName;
                        
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        return View("Index", user.Profil);
                    }
                    }
            }

            return RedirectToAction("Index");
        }
        }
    }
