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

        //public async Task<IEnumerable<PurchaseDetail>> GetUserPurchases(string userId)
        //{
        //    if (userId.Equals(0))
        //    {
        //        return new List<PurchaseDetail>(); // Si no hay usuario, no hay compras
        //    }

        //    try
        //    {
        //        int setUsuarioId = int.Parse(userId);
        //        return await _context.PurchaseDetails
        //                             .Include(pd => pd.Album)
        //                             .Where(pd => pd.Client_Id == setUsuarioId)
        //                             .OrderByDescending(pd => pd.PurchaseDate)
        //                             .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception in GetUserPurchases: {ex.Message}");
        //        // Devolver una lista vacía y registrar el error es mejor que relanzar en muchos casos de lectura
        //        return new List<PurchaseDetail>();
        //    }
        //}

        public async Task<IEnumerable<PurchaseDetail>> GetUserPurchases(string userLoginId)
        {
            if (string.IsNullOrEmpty(userLoginId))
            {
                return new List<PurchaseDetail>();
            }

            try
            {
                // Incluir el Álbum y luego las Canciones del Álbum
                return await _context.PurchaseDetails
                                     .Include(pd => pd.Album) // Carga el objeto Album
                                         .ThenInclude(a => a.Songs.OrderBy(s => s.Name)) // Y luego, las canciones de ese álbum, ordenadas por nombre
                                     .Where(pd => pd.Client_Id == int.Parse(userLoginId))
                                     .OrderByDescending(pd => pd.PurchaseDate)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserPurchases: {ex.Message}");
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

        public async Task<(bool success, string message)> BuyMultipleAlbums(List<int> albumIds, string userLoginId)
        {
            if (string.IsNullOrEmpty(userLoginId))
            {
                return (false, "Debe iniciar sesión para realizar una compra.");
            }
            if (albumIds == null || !albumIds.Any())
            {
                return (false, "Debe seleccionar al menos un álbum para comprar.");
            }

            // Obtener información del cliente
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == int.Parse(userLoginId));

            // Obtener los álbumes seleccionados
            var selectedAlbums = await _context.AlbumSets
                                               .Where(a => albumIds.Contains(a.Id))
                                               .ToListAsync();

            if (selectedAlbums.Count != albumIds.Count)
            {
                // Si no se encontraron todos los álbumes, algunos IDs eran inválidos.
                return (false, "Algunos álbumes seleccionados no son válidos o no existen.");
            }

            // Obtener los álbumes que el usuario ya compró para evitar duplicados
            var purchasedAlbumIds = await GetPurchasedAlbumIds(userLoginId);

            var newPurchases = new List<PurchaseDetail>();
            decimal totalPurchaseAmount = 0;
            int albumsSuccessfullyPurchased = 0;

            foreach (var album in selectedAlbums)
            {
                if (!purchasedAlbumIds.Contains(album.Id)) // Solo comprar si no lo tiene ya
                {
                    var purchaseDetail = new PurchaseDetail
                    {
                        Client_Id = client.Id,
                        Album_Id = album.Id,
                        Total = album.Price, // Cada PurchaseDetail registra el precio de ese álbum
                        PurchaseDate = DateTime.Now
                    };
                    newPurchases.Add(purchaseDetail);
                    totalPurchaseAmount += album.Price;
                    albumsSuccessfullyPurchased++;
                }
            }

            if (!newPurchases.Any())
            {
                return (true, "Ya posees todos los álbumes seleccionados o no hay nuevos álbumes para comprar.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.PurchaseDetails.AddRangeAsync(newPurchases);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, $"¡Has comprado {albumsSuccessfullyPurchased} álbum(es) por un total de {totalPurchaseAmount.ToString("C")} exitosamente!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Exception in BuyMultipleAlbums: {ex.Message}");
                    return (false, $"Error al procesar la compra múltiple: {ex.Message}");
                }
            }
        }
    }
}