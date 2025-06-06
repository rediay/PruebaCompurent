using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicRadioInc.Models;
using MusicRadioInc.Filters;

namespace MusicRadioInc.Controllers
{
    [CustomAuthorize("Admin", "Editor")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Recuperar el nombre del usuario de la sesión
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            return View();
        }

        [CustomAuthorize("Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        // Acción para acceso denegado
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
