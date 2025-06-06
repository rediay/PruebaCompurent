using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Data;
using MusicRadioInc.Models;
namespace MusicRadioInc.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
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

                    var user = await _context.Clients
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.UserLoginId == model.UserLoginId);

                    //bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

                    if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                    {
                        // Login exitoso
                        HttpContext.Session.SetString("UserLoginId", user.UserLoginId);
                        HttpContext.Session.SetString("UserName", user.Name ?? user.UserLoginId); // Almacenar el nombre o el ID de usuario
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
                try
                {
                    // Verificar si el UserLoginId ya existe
                    if (await _context.Clients.AnyAsync(u => u.UserLoginId == model.UserLoginId))
                    {
                        ModelState.AddModelError("UserLoginId", "Este ID de usuario ya está registrado.");
                        return View(model);
                    }

                    // Puedes agregar aquí un hashing de la contraseña si es necesario para mayor seguridad
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    model.Password = hashedPassword;
                    model.RolId = 2;

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    ViewBag.SuccessMessage = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Index", "Login"); // Redirigir al login después del registro
                }
                catch (DbUpdateException ex)
                {
                    // Controlar excepciones específicas de la base de datos, como violación de índice único
                    Console.WriteLine($"Error al registrar usuario: {ex.InnerException?.Message}");
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al registrar el usuario. Asegúrese de que el ID de usuario no esté duplicado.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error general al registrar usuario: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al registrar el usuario.");
                }
            }
            return View(model);
        }
    }
}
