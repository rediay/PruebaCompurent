using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;

namespace MusicRadioInc.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client> ValidateUserCredentials(LoginViewModel model)
        {
            var user = await _context.Clients
                                     .Include(u => u.Rol)
                                     .FirstOrDefaultAsync(u => u.UserLoginId == model.UserLoginId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return null; // Credenciales inválidas
            }

            return user; // Credenciales válidas
        }

        public async Task<bool> RegisterNewUser(Client user)
        {
            if (await IsUserLoginIdTaken(user.UserLoginId))
            {
                return false; // UserLoginId ya existe
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.RolId = 2; // Asignar rol "User" por defecto

            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al registrar usuario (DB): {ex.InnerException?.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al registrar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsUserLoginIdTaken(string userLoginId)
        {
            return await _context.Usuarios.AnyAsync(u => u.UserLoginId == userLoginId);
        }
    }
}
