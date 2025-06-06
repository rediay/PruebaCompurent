using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;

namespace MusicRadioInc.Services.Implementations
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;

        public PurchaseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> BuyAlbum(int albumId, string userLoginId)
        {
            if (string.IsNullOrEmpty(userLoginId))
            {
                return (false, "Debe iniciar sesión para realizar una compra.");
            }

            try
            {
                var album = await _context.AlbumSets.FindAsync(albumId);
                if (album == null)
                {
                    return (false, "Álbum no encontrado.");
                }

                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserLoginId == userLoginId);
                //if (client == null)
                //{
                //    var user = await _context.Clients.FirstOrDefaultAsync(u => u.UserLoginId == userLoginId);
                //    if (user == null)
                //    {
                //        return (false, "Información de usuario no disponible para crear cliente.");
                //    }

                //    client = new Client
                //    {
                //        Id = user.Id,
                //        Name = user.Name ?? user.UserLoginId,
                //        Mail = user.Mail,
                //        Direction = user.Direction,
                //        Phone = user.Phone
                //    };
                //    _context.Clients.Add(client);
                //    await _context.SaveChangesAsync();
                //}

                var purchaseDetail = new PurchaseDetail
                {
                    Client_Id = client.Id,
                    Album_Id = album.Id,
                    Total = album.Price,
                    PurchaseDate = DateTime.Now
                };

                _context.PurchaseDetails.Add(purchaseDetail);
                await _context.SaveChangesAsync();

                return (true, $"¡Has comprado '{album.Name}' exitosamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in BuyAlbum: {ex.Message}");
                return (false, $"Error al procesar la compra: {ex.Message}");
            }
        }

        public async Task<IEnumerable<PurchaseDetail>> GetUserPurchases(string userId)
        {
            if (userId.Equals(0))
            {
                return new List<PurchaseDetail>(); // Si no hay usuario, no hay compras
            }

            try
            {
                int setUsuarioId = int.Parse(userId);
                return await _context.PurchaseDetails
                                     .Include(pd => pd.Album)
                                     .Where(pd => pd.Client_Id == setUsuarioId)
                                     .OrderByDescending(pd => pd.PurchaseDate)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserPurchases: {ex.Message}");
                // Devolver una lista vacía y registrar el error es mejor que relanzar en muchos casos de lectura
                return new List<PurchaseDetail>();
            }
        }

        public async Task<HashSet<int>> GetPurchasedAlbumIds(string userLoginId)
        {
            if (string.IsNullOrEmpty(userLoginId))
            {
                return new HashSet<int>();
            }

            try
            {
                // Obtener los IDs de los álbumes que el usuario ya compró
                var purchasedIds = await _context.PurchaseDetails
                                                 .Where(pd => pd.Client_Id == int.Parse(userLoginId))
                                                 .Select(pd => pd.Album_Id)
                                                 .Distinct() // Asegurarse de que cada ID sea único
                                                 .ToListAsync();

                return new HashSet<int>(purchasedIds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetPurchasedAlbumIds: {ex.Message}");
                return new HashSet<int>();
            }
        }
    }
}