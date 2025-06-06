using MusicRadioInc.Models;

namespace MusicRadioInc.Interfaces
{
    public interface IPurchaseService
    {
        Task<(bool success, string message)> BuyAlbum(int albumId, string userLoginId);
        Task<IEnumerable<PurchaseDetail>> GetUserPurchases(string userId);
        Task<HashSet<int>> GetPurchasedAlbumIds(string userLoginId);
    }
}
