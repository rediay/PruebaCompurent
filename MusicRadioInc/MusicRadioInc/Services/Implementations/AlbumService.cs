using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;
using System.Data;

namespace MusicRadioInc.Services.Implementations
{
    public class AlbumService : IAlbumService
    {
        private readonly ApplicationDbContext _context;

        public AlbumService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlbumSet>> GetAllAlbums()
        {
            return await _context.AlbumSets.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<AlbumSet> GetAlbumById(int id)
        {
            return await _context.AlbumSets
                                 .Include(a => a.Songs.OrderBy(s => s.Name))
                                 .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool success, string message, int newAlbumId)> SaveAlbum(AlbumSet album)
        {
            string operation = (album.Id == 0) ? "Insert" : "Update";
            int resultId = 0;

            try
            {
                var albumIdParam = new SqlParameter("@AlbumId", SqlDbType.Int) { Value = album.Id == 0 ? DBNull.Value : album.Id };
                var albumNameParam = new SqlParameter("@AlbumName", SqlDbType.NVarChar, 255) { Value = album.Name };
                var albumPriceParam = new SqlParameter("@AlbumPrice", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = album.Price };
                var operationParam = new SqlParameter("@Operation", SqlDbType.NVarChar, 10) { Value = operation };

                // Para obtener el valor del parámetro de salida, necesitamos usar SqlCommand directamente.
                // Se requiere una conexión abierta.
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC @ResultId = sp_ManageAlbum @Operation, @AlbumId, @AlbumName, @AlbumPrice";
                        command.Parameters.Add(new SqlParameter("@ResultId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(operationParam);
                        command.Parameters.Add(albumIdParam);
                        command.Parameters.Add(albumNameParam);
                        command.Parameters.Add(albumPriceParam);

                        await command.ExecuteNonQueryAsync();

                        resultId = (int)command.Parameters["@ResultId"].Value;
                    }
                }
                finally
                {
                    await connection.CloseAsync(); // Asegurarse de cerrar la conexión
                }

                return (true, $"Álbum {(album.Id == 0 ? "creado" : "actualizado")} exitosamente.", resultId);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SqlException in SaveAlbum: {ex.Message}");
                return (false, $"Error de base de datos al guardar álbum: {ex.Message}", 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SaveAlbum: {ex.Message}");
                return (false, $"Ocurrió un error inesperado al guardar el álbum: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> DeleteAlbum(int id)
        {
            try
            {
                var albumIdParam = new SqlParameter("@AlbumId", SqlDbType.Int) { Value = id };
                var operationParam = new SqlParameter("@Operation", SqlDbType.NVarChar, 10) { Value = "Delete" };

                // Usar SqlCommand directamente para obtener el parámetro de salida si es necesario
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "EXEC @ResultId = sp_ManageAlbum @Operation, @AlbumId";
                        command.Parameters.Add(new SqlParameter("@ResultId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(operationParam);
                        command.Parameters.Add(albumIdParam);

                        await command.ExecuteNonQueryAsync();
                        // El valor del parámetro de salida aquí no es tan crítico para DELETE,
                        // pero se puede obtener si se desea para logueo.
                    }
                }
                finally
                {
                    await connection.CloseAsync(); // Asegurarse de cerrar la conexión
                }

                return (true, "Álbum eliminado exitosamente.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SqlException in DeleteAlbum: {ex.Message}");
                return (false, $"Error de base de datos al eliminar álbum: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteAlbum: {ex.Message}");
                return (false, $"Ocurrió un error inesperado al eliminar el álbum: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> AddSongToAlbum(int albumId, string songName)
        {
            if (string.IsNullOrWhiteSpace(songName))
            {
                return (false, "El nombre de la canción es obligatorio.");
            }

            try
            {
                var album = await _context.AlbumSets.FindAsync(albumId);
                if (album == null)
                {
                    return (false, "Álbum no encontrado.");
                }

                var newSong = new SongSet { Name = songName, Album_Id = albumId };
                _context.Add(newSong);
                await _context.SaveChangesAsync();
                return (true, "Canción agregada exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddSongToAlbum: {ex.Message}");
                return (false, $"Error al agregar canción: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> RemoveSongFromAlbum(int songId)
        {
            try
            {
                var song = await _context.SongSets.FindAsync(songId);
                if (song == null)
                {
                    return (false, "Canción no encontrada.");
                }

                _context.Remove(song);
                await _context.SaveChangesAsync();
                return (true, "Canción eliminada exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RemoveSongFromAlbum: {ex.Message}");
                return (false, $"Error al eliminar canción: {ex.Message}");
            }
        }
    }
}