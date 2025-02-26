using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<WebUser> _userManager;
        private readonly SignInManager<WebUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;


        public AccountController(UserManager<WebUser> userManager, SignInManager<WebUser> signInManager, ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new WebUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("UserPage");
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var role = await _userManager.GetRolesAsync(user);

                    if (role.Contains("User"))
                    {
                        return RedirectToAction("UserPage");
                    }
                    else if (role.Contains("Caseworker"))
                    {
                        return RedirectToAction("CaseworkerPage");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login failed.");
                }
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult UserPage()
        {
            return View();
        }


        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserPage(string geoJson, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("GeoJson og beskrivelse må legges til");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Kunne ikke finne bruker");
                }

                // Finn kommuneinformasjon
                var (Kommunenummer, Kommunenavn, Fylkesnavn) = await FinnKommuneAsync(geoJson);

                // Logg kommuneinformasjon
                _logger.LogInformation($"Kommunenummer: {Kommunenummer}, Kommunenavn: {Kommunenavn}");

                // Se om vi fant en kommune
                if (string.IsNullOrEmpty(Kommunenummer) || string.IsNullOrEmpty(Kommunenavn))
                {
                    return BadRequest("Kunne ikke finne kommuneinformasjon. Vennligst sjekk koordinatene.");
                }

                // Definerer en ny GeoChange og legger den til i databasen
                var newChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description,
                    UserId = userId,
                    Kommunenummer = Kommunenummer,
                    Kommunenavn = Kommunenavn,
                    Fylkesnavn = Fylkesnavn,
                };

                _context.GeoChanges.Add(newChange);
                await _context.SaveChangesAsync();

                // Redirigerer til oversikten over endringer
                return RedirectToAction("ReportOverview");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "En feil oppstod ved lagring av endring");
                return StatusCode(500, "Intern serverfeil");
            }
        }


        // Metode som henter kommuneinfo fra GeoJSON
        private async Task<(string Kommunenummer, string Kommunenavn, string Fylkesnavn)> FinnKommuneAsync(string geoJson)
        {
            var kommunefinner = HttpContext.RequestServices.GetRequiredService<Kommunefinner>();

            // Spesifiserer at alle tre tingene skal hentes 
            var result = await kommunefinner.FinnKommuneFraGeoJsonAsync(geoJson);
            return (result.Kommunenummer, result.Kommunenavn, result.Fylkesnavn);
        }


        [Authorize(Roles = "User")]
        [HttpGet]
            public async Task<IActionResult> ReportOverview()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var userChanges = await _context.GeoChanges
                .Where(change => change.UserId == user.Id)
                .ToListAsync();

            return View(userChanges);
        }


        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoChange = await _context.GeoChanges
                .FirstOrDefaultAsync(m => m.Id == id);
            if (geoChange == null)
            {
                return NotFound();
            }

            return View(geoChange);
        }


        [Authorize(Roles = "User")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange != null)
            {
                _context.GeoChanges.Remove(geoChange);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ReportOverview", "Account");
        }

        [Authorize(Roles = "Caseworker")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var change = await _context.GeoChanges.FindAsync(id);
            if (change != null)
            {
                change.IsApproved = true;
                await _context.SaveChangesAsync();
            }
                return RedirectToAction("CaseworkerPage");
        }


        [Authorize(Roles = "Caseworker")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var change = await _context.GeoChanges.FindAsync(id);
            if (change != null)
            {
                change.IsApproved = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CaseworkerPage");
        }

        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public IActionResult CaseworkerPage()
        {
            var changes_db = _context.GeoChanges.ToList();
            if (changes_db == null || !changes_db.Any())
            {
                return View(new List<GeoChange>());
            }
            return View(changes_db);
        }

        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public async Task<IActionResult> CaseworkerDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var geoChange = await _context.GeoChanges
                .FirstOrDefaultAsync(m => m.Id == id);
            if (geoChange == null)
            {
                return NotFound();
            }

            return View(geoChange);
        }
        [Authorize(Roles = "Caseworker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaseworkerDelete(int id)
        {
            var geoChange = await _context.GeoChanges.FindAsync(id);
            if (geoChange != null)
            {
                _context.GeoChanges.Remove(geoChange);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CaseworkerPage", "Account");
        }

        public async Task<IActionResult> ApprovedReports()
        {
            var geoChanges = await _context.GeoChanges.Where(c => c.IsApproved == true).ToListAsync();
            return View(geoChanges);
        }


        public async Task<IActionResult> DeniedReports()
        {
            var geoChanges = await _context.GeoChanges.Where(c => c.IsApproved == false).ToListAsync();
            return View(geoChanges);
        }


        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [Authorize(Roles = "Caseworker")]
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("CaseworkerPage");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }
        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> DeleteSelf()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost, ActionName("DeleteSelf")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelfConfirmed()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }
    }
}