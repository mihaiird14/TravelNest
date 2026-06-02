using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelNest.ViewModels;

namespace TravelNest.Controllers
{
    [Authorize]
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MapController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null) 
                return View(new List<string>());
            var grupuriVizibile = await _context.TravelGroups
                .Include(g => g.Locatii)
                .Include(g => g.ListaParticipanti)
                .Where(g => (g.AdminId == profil.Id ||
                             g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .ToListAsync();

            var hartiAscunse = await _context.HartiAscunse
                .Where(h => h.ProfilId == profil.Id)
                .Select(h => h.TravelGroupId)
                .ToListAsync();

            var coduri = grupuriVizibile
                .Where(g => !hartiAscunse.Contains(g.Id))
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToList();
         
         
            var dupafiltru = grupuriVizibile.Where(g => !hartiAscunse.Contains(g.Id)).ToList();
            var manualCountries = await _context.TariVizitate
                .Where(t => t.ProfilId == profil.Id)
                .Select(t => t.CodTara.ToUpper())
                .Distinct()
                .ToListAsync();

            var allCountries = coduri.Union(manualCountries).Distinct().ToList();
            
            return View(allCountries);
        }
        public async Task<IActionResult> FriendsMap()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils
                .Include(p => p.Followers)
                .Include(p => p.Following)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profil == null)
                return View(new FriendsMapViewModel());

            // Mutual follow 
            var followingIds = profil.Following.Select(f => f.FollowedId).ToHashSet();
            var followerIds = profil.Followers.Select(f => f.FollowerId).ToHashSet();
            var mutualIds = followingIds.Intersect(followerIds).ToHashSet();

            // Prietenii cu ShowOnFriendsMap = true
            var prieteni = await _context.Profils
                .Include(p => p.User)
                .Where(p => mutualIds.Contains(p.Id) && p.ShowOnFriendsMap)
                .ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayFull = DateTime.Today;

            var vm = new FriendsMapViewModel();

            foreach (var prieten in prieteni)
            {
                // Toate grupurile prietenului (admin sau member confirmat)
                var grupuri = await _context.TravelGroups
                    .Include(g => g.Locatii)
                    .Include(g => g.Zboruri)
                    .Include(g => g.ListaParticipanti)
                    .Where(g => g.AdminId == prieten.Id ||
                                g.ListaParticipanti.Any(m => m.ProfilId == prieten.Id && m.Confirmare == "MEMBER"))
                    .ToListAsync();

                var dto = new FriendMarkerDto
                {
                    ProfilId = prieten.Id,
                    Nume = prieten.User?.UserName ?? $"User {prieten.Id}",
                    ImagineProfil = prieten.ImagineProfil ?? "/images/profilDefault.png"
                };

                foreach (var grup in grupuri)
                {
                    
                    string status;

                    if (grup.DataPlecare.HasValue && grup.DataIntoarcere.HasValue)
                    {
                        if (today < grup.DataPlecare.Value)
                            status = "upcoming";
                        else if (today > grup.DataIntoarcere.Value)
                            continue; // vacanta terminata, skip
                        else
                            status = "active";
                    }
                    else
                    {
                        // Fara date => tratam ca activ
                        status = "active";
                    }

                    var locatii = grup.Locatii
                        .Where(l => !string.IsNullOrEmpty(l.CodTara))
                        .ToList();

                    if (!locatii.Any()) continue;

                 
                    FlightDto? zborAzi = null;
                    var zborAziDb = grup.Zboruri
                        .FirstOrDefault(z => z.DataPlecare.Date == todayFull.Date);

                    if (zborAziDb != null)
                    {
                        zborAzi = new FlightDto
                        {
                            OrasPlecare = zborAziDb.OrasPlecare,
                            OrasSosire = zborAziDb.OrasSosire,
                            AeroportPlecare = zborAziDb.AeroportPlecare,
                            AeroportSosire = zborAziDb.AeroportSosire,
                            NumarZbor = zborAziDb.NumarZbor,
                            DataPlecare = zborAziDb.DataPlecare,
                            DataSosire = zborAziDb.DataSosire
                        };
                    }

                  
                    string codTara;

                    if (status == "upcoming")
                    {
                       
                        codTara = locatii
                            .OrderBy(l => l.CheckIn ?? DateOnly.MaxValue)
                            .ThenBy(l => l.Id)
                            .First().CodTara!.ToUpper();
                    }
                    else 
                    {
                        if (grup.Zboruri.Any())
                        {
                            
                            var ultimulZborAterizat = grup.Zboruri
                                .Where(z => z.DataSosire <= DateTime.Now)
                                .OrderByDescending(z => z.DataSosire)
                                .FirstOrDefault();

                            if (ultimulZborAterizat != null)
                            {
                               
                                var locatieDupaSosire = locatii
                                    .Where(l => l.CheckIn.HasValue &&
                                                l.CheckIn.Value >= DateOnly.FromDateTime(ultimulZborAterizat.DataSosire))
                                    .OrderBy(l => l.CheckIn)
                                    .ThenBy(l => l.Id)
                                    .FirstOrDefault();

                                codTara = (locatieDupaSosire?.CodTara ?? locatii
                                    .OrderBy(l => l.CheckIn ?? DateOnly.MaxValue)
                                    .ThenBy(l => l.Id)
                                    .First().CodTara)!.ToUpper();
                            }
                            else
                            {
                                codTara = locatii
                                    .OrderBy(l => l.CheckIn ?? DateOnly.MaxValue)
                                    .ThenBy(l => l.Id)
                                    .First().CodTara!.ToUpper();
                            }
                        }
                        else
                        {
                            codTara = locatii
                                .OrderBy(l => l.Id)
                                .First().CodTara!.ToUpper();
                        }
                    }

                    dto.Grupuri.Add(new FriendGroupDto
                    {
                        GrupId = grup.Id,
                        NumeGrup = grup.Nume,
                        Status = status,
                        CodTara = codTara,
                        ZborAzi = zborAzi,
                        Orase = locatii.Select(l => l.Locatie).Distinct().ToList()
                    });
                }

                if (dto.Grupuri.Any())
                    vm.Friends.Add(dto);
            }

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> FriendsMapDebug()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils
                .Include(p => p.Followers)
                .Include(p => p.Following)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profil == null)
                return Json(new { error = "Profil null" });

            var followingIds = profil.Following.Select(f => f.FollowedId).ToHashSet();
            var followerIds = profil.Followers.Select(f => f.FollowerId).ToHashSet();
            var mutualIds = followingIds.Intersect(followerIds).ToHashSet();

            var prieteni = await _context.Profils
                .Where(p => mutualIds.Contains(p.Id))
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var result = new List<object>();

            foreach (var p in prieteni)
            {
                var grupuri = await _context.TravelGroups
                    .Include(g => g.Locatii)
                    .Include(g => g.Zboruri)
                    .Include(g => g.ListaParticipanti)
                    .Where(g => g.AdminId == p.Id ||
                                g.ListaParticipanti.Any(m => m.ProfilId == p.Id && m.Confirmare == "MEMBER"))
                    .ToListAsync();

                result.Add(new
                {
                    profilId = p.Id,
                    showOnMap = p.ShowOnFriendsMap,
                    followingCount = followingIds.Count,
                    followerCount = followerIds.Count,
                    mutualIds = mutualIds.ToList(),
                    grupuri = grupuri.Select(g => new
                    {
                        g.Id,
                        g.Nume,
                        dataPlecare = g.DataPlecare?.ToString(),
                        dataIntoarcere = g.DataIntoarcere?.ToString(),
                        today = today.ToString(),
                        isUpcoming = g.DataPlecare.HasValue && today < g.DataPlecare.Value,
                        locatii = g.Locatii.Select(l => new { l.Id, l.CodTara, l.Locatie, checkIn = l.CheckIn?.ToString() }),
                        zboruri = g.Zboruri.Select(z => new { z.Id, z.DataPlecare, z.DataSosire, z.AeroportPlecare, z.AeroportSosire })
                    })
                });
            }

            return Json(new
            {
                myProfilId = profil.Id,
                followingIds = followingIds.ToList(),
                followerIds = followerIds.ToList(),
                mutualIds = mutualIds.ToList(),
                prieteniGasiti = prieteni.Count,
                prieteni = result
            });
        }
        [HttpGet]
        public async Task<IActionResult> IstoricTari()
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null) 
                return Json(new List<string>());
            var grupuriVizibile = await _context.TravelGroups
                .Include(g => g.Locatii)
                .Include(g => g.ListaParticipanti)
                .Where(g => (g.AdminId == profil.Id ||
                             g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .ToListAsync();

            var hartiAscunse = await _context.HartiAscunse
                .Where(h => h.ProfilId == profil.Id)
                .Select(h => h.TravelGroupId)
                .ToListAsync();

            var coduri = grupuriVizibile
                .Where(g => !hartiAscunse.Contains(g.Id))
                .SelectMany(g => g.Locatii)
                .Where(l => !string.IsNullOrEmpty(l.CodTara))
                .Select(l => l.CodTara.ToUpper())
                .Distinct()
                .ToList();

            var uniqueCountries = coduri.Distinct().ToList();
            
            return Json(uniqueCountries);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCountry([FromBody] CountryToggleRequest request)
        {
            var userId = _userManager.GetUserId(User);
            var profil = await _context.Profils.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profil == null)
                return Json(new { success = false, message = "Profil not found" });

            var codTara = request.CountryCode.ToUpper();

            var existsInGroup = await _context.TravelGroups
                .Where(g => !_context.HartiAscunse.Any(h => h.ProfilId == profil.Id && h.TravelGroupId == g.Id) &&
                           (g.AdminId == profil.Id ||
                            g.ListaParticipanti.Any(m => m.ProfilId == profil.Id && m.Confirmare == "MEMBER")))
                .Include(g => g.Locatii)
                .SelectMany(g => g.Locatii)
                .AnyAsync(l => l.CodTara.ToUpper() == codTara);
            var manualEntry = await _context.TariVizitate
                .FirstOrDefaultAsync(t => t.ProfilId == profil.Id && t.CodTara == codTara);

            if (request.IsAdding)
            {
                if (manualEntry == null)
                {
                    _context.TariVizitate.Add(new TaraVizitata
                    {
                        ProfilId = profil.Id,
                        CodTara = codTara,
                        AdaugatManual = true
                    });
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Country added" });
                }
            }
            else
            {
                
                if (existsInGroup)
                    return Json(new { success = false, message = "Cannot remove country that is part of a travel group" });

                if (manualEntry != null)
                {
                    _context.TariVizitate.Remove(manualEntry);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Country removed" });
                }
            }

            return Json(new { success = false, message = "Invalid operation" });
        }
    }

    public class CountryToggleRequest
    {
        public string CountryCode { get; set; }
        public bool IsAdding { get; set; }
    }
}