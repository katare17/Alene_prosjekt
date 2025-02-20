using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<WebUser> _userManager;

        private static List<PositionModel> positions = new List<PositionModel>();

        // Database connection
        private readonly ApplicationDbContext _context;

        //In-memory storage
        private static List<AreaChange> changes = new List<AreaChange>();

        //Api services
        private readonly IKommuneInfoService _KommuneInfoService;
        private readonly IStedsnavnService _StedsnavnService;

        public HomeController(ILogger<HomeController> logger, IKommuneInfoService kommuneInfoService, IStedsnavnService stedsnavnService, ApplicationDbContext context)
        {
            _logger = logger;
            _KommuneInfoService = kommuneInfoService;
            _StedsnavnService = stedsnavnService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View("Index");
        }


        // Register area change with GeoJson, description
        [HttpPost]
        public IActionResult RegisterAreaChange(string geoJson, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("Invalid data.");
                }

                var newGeoChange = new GeoChange
                {
                    GeoJson = geoJson,
                    Description = description
                };

                // Save to database
                _context.GeoChanges.Add(newGeoChange);
                _context.SaveChanges();

                // Redirect to the overview of changes
                return RedirectToAction("AreaChangeOverview");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}, Inner Exeption: {ex.InnerException?.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<(string MunicipalityNumber, string MunicipalityName, string CountyName)> FindMunicipalityAsync(string geoJson)
        {
            var municipalityFinderService = HttpContext.RequestServices.GetRequiredService<KommuneInfoService>();

            // Explicitly return all three elements
            var result = await municipalityFinderService.FindMunicipalityFromGeoJsonAsync(geoJson);
            return (result.MunicipalityNumber, result.MunicipalityName, result.CountyName);
        }

        [HttpGet]
        public IActionResult AreaChangeOverview()
        {
            var changes_db = _context.GeoChanges.ToList();
            return View(changes_db);
        }

        [HttpPost]
        public async Task<IActionResult> KommuneInfo(string kommuneNr)
        {
            if (string.IsNullOrEmpty(kommuneNr))
            {
                ViewData["Error"] = "Please enter a valid Kommune Number.";
                return View("Index2");
            }

            var kommuneInfo = await _KommuneInfoService.GetKommuneInfoAsync(kommuneNr);
            if (kommuneInfo != null)
            {
                var viewModel = new KommuneInfoViewModel
                {
                    Kommunenavn = kommuneInfo.Kommunenavn,
                    Kommunenummer = kommuneInfo.Kommunenummer,
                    Fylkesnavn = kommuneInfo.Fylkesnavn,
                    SamiskForvaltningsomrade = kommuneInfo.SamiskForvaltningsomrade,
                };
                return View("KommuneInfo", viewModel);
            }
            else
            {
                ViewData["Error"] = $"No results found for Kommune Number '{kommuneNr}'.";
                return View("Index2");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Stedsnavn(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                ViewData["Error"] = "Please enter a valid place name.";
                return View("Index2");
            }
            var stedsnavnResponse = await _StedsnavnService.GetStedsnavnAsync(searchTerm);
            if(stedsnavnResponse? .Navn != null && stedsnavnResponse.Navn.Any())
            {
                var viewModel = stedsnavnResponse.Navn.Select(n => new StedsnavnViewModel
                    {
                    Skrivemåte = n.Skrivemåte,
                        Navneobjekttype = n.Navneobjekttype,
                        Språk = n.Språk,
                        Navnestatus = n.Navnestatus
                }).ToList();
                return View("Stedsnavn", viewModel);
            }
            else
            {
                ViewData["Error"] = $"No result found for '{searchTerm}'."; 
                return View("Index2");
            }
        }


        [HttpGet]
        public IActionResult CorrectMap()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CorrectMap(PositionModel model)
        {
            if (ModelState.IsValid)
                    {
                positions.Add(model);

                return View("CorrectionOverview", positions);
                }
            return View();
        }

        [HttpGet]
        public IActionResult CorrectionOverview()
        {
            return View(positions);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RegisterAreaChange()
        {
            return View();
        }


  
    }
}
