using Microsoft.AspNetCore.Mvc;
using MusicRadioInc.Filters;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Models;

namespace MusicRadioInc.Controllers
{
    [CustomAuthorize("Admin")]
    public class AdminController : Controller
    {
        private readonly IAlbumService _albumService;

        public AdminController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: Admin/ManageAlbums
        public async Task<IActionResult> ManageAlbums()
        {
            var albums = await _albumService.GetAllAlbums();
            return View(albums);
        }

        // GET: Admin/EditAlbum (para crear o editar)
        public async Task<IActionResult> EditAlbum(int? id)
        {
            if (id == null || id == 0) // Considerar 0 para "nuevo"
            {
                return View(new AlbumSet());
            }

            var album = await _albumService.GetAlbumById(id.Value);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        // POST: Admin/SaveAlbum
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAlbum(AlbumSet album)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a cargar las canciones si es una edición
                if (album.Id != 0)
                {
                    var existingAlbum = await _albumService.GetAlbumById(album.Id);
                    if (existingAlbum != null)
                    {
                        album.Songs = existingAlbum.Songs;
                    }
                }
                return View("EditAlbum", album);
            }

            var (success, message, newAlbumId) = await _albumService.SaveAlbum(album);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(ManageAlbums));
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
                return View("EditAlbum", album);
            }
        }

        // POST: Admin/DeleteAlbum
        [HttpPost, ActionName("DeleteAlbum")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAlbumConfirmed(int id)
        {
            var (success, message) = await _albumService.DeleteAlbum(id);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message; // Usar TempData para errores en redirección
            }
            return RedirectToAction(nameof(ManageAlbums));
        }

        // POST: Admin/AddSongToAlbum
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSongToAlbum(int albumId, string songName)
        {
            var (success, message) = await _albumService.AddSongToAlbum(albumId, songName);

            if (!success)
            {
                // Si hay un error, TempData["ErrorMessage"] será usado en la vista.
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["SuccessMessage"] = message;
            }
            return RedirectToAction(nameof(EditAlbum), new { id = albumId });
        }

        // POST: Admin/RemoveSongFromAlbum
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSongFromAlbum(int songId, int albumId)
        {
            var (success, message) = await _albumService.RemoveSongFromAlbum(songId);

            if (!success)
            {
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["SuccessMessage"] = message;
            }
            return RedirectToAction(nameof(EditAlbum), new { id = albumId });
        }
    }
}