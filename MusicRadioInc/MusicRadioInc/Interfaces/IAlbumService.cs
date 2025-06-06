using MusicRadioInc.Models;

namespace MusicRadioInc.Interfaces
{
    public interface IAlbumService
    {
        Task<IEnumerable<AlbumSet>> GetAllAlbums();
        Task<AlbumSet> GetAlbumById(int id);
        Task<(bool success, string message, int newAlbumId)> SaveAlbum(AlbumSet album); // Retorna si fue exitoso, mensaje y el ID del nuevo álbum
        Task<(bool success, string message)> DeleteAlbum(int id);
        Task<(bool success, string message)> AddSongToAlbum(int albumId, string songName);
        Task<(bool success, string message)> RemoveSongFromAlbum(int songId);
    }
}
