using MusicRadioInc.Models;

namespace MusicRadioInc.ViewModels
{
    public class StoreIndexViewModel
    {
        public IEnumerable<AlbumSet>? AvailableAlbums { get; set; }
        public HashSet<int>? PurchasedAlbumIds { get; set; } // Usamos HashSet para búsquedas rápidas
    }
}
