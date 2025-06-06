using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;
namespace MusicRadioInc.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public LoginController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: Login/Index
        public IActionResult Index()
        {
            // Si el usuario ya está logueado, redirigir al Home
            if (HttpContext.Session.GetString("UserLoginId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login/Index
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra ataques CSRF
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var user = await _authService.ValidateUserCredentials(model);

                    if (user != null)
                    {
                        // Login exitoso
                        HttpContext.Session.SetString("UserLoginId", user.Id.ToString());
                        HttpContext.Session.SetString("UserName", user.UserLoginId ?? user.Name); // Almacenar el nombre o el ID de usuario
                        HttpContext.Session.SetString("UserRole", user.Rol.NombreRol);

                        // Redirigir al usuario a la página de inicio o a un dashboard
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "ID de usuario o contraseña incorrectos.");
                    }
                }
                catch (Exception ex)
                {
                    // Control de excepciones
                    Console.WriteLine($"Error de base de datos en login: {ex.Message}"); // Para depuración
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al intentar iniciar sesión. Por favor, inténtelo de nuevo.");
                }
            }
            // Si el modelo no es válido o el login falla, volver a la vista con errores
            return View(model);
        }

        // GET: Login/Logout
        public IActionResult Logout()
        {
            // Limpiar la sesión
            HttpContext.Session.Clear();
            // Redirigir a la página de login
            return RedirectToAction("Index", "Login");
        }

        // GET: Login/Register (Opcional: para registrar nuevos usuarios)
        public IActionResult Register()
        {
            return View();
        }

        // POST: Login/Register (Opcional: para registrar nuevos usuarios)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Client model)
        {
            if (ModelState.IsValid)
            {
                var registrationSuccess = await _authService.RegisterNewUser(model);
                if (registrationSuccess)
                {
                    TempData["SuccessMessage"] = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    ModelState.AddModelError("UserLoginId", "Este ID de usuario ya está registrado o ocurrió un error.");
                }
            }
            return View(model);
        }
    }
}
