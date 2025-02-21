using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, IKommuneInfoService kommuneInfoService, IStedsnavnService stedsnavnService, ApplicationDbContext context, UserManager<WebUser> userManager)
        {
            _logger = logger;
            _KommuneInfoService = kommuneInfoService;
            _StedsnavnService = stedsnavnService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View("Index");
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
    }
}
