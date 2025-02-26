using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<WebUser> _userManager;

        // Databasekobling
        private readonly ApplicationDbContext _context;

        // Minne-lagring
        private static List<AreaChange> changes = new List<AreaChange>();


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<WebUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
