using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class MinController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
