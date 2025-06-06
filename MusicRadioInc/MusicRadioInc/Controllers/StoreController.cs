using Microsoft.AspNetCore.Mvc;
using MusicRadioInc.Filters;
using MusicRadioInc.Interfaces;
using MusicRadioInc.ViewModels;

namespace MusicRadioInc.Controllers
{
    [CustomAuthorize("User", "Admin", "Editor")]
    public class StoreController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IPurchaseService _purchaseService;

        public StoreController(IAlbumService albumService, IPurchaseService purchaseService)
        {
            _albumService = albumService;
            _purchaseService = purchaseService;
        }

        // GET: Store/Index (Mostrar todos los álbumes disponibles para compra)
        public async Task<IActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbums();

            var userLoginId = HttpContext.Session.GetString("UserLoginId");

            HashSet<int> purchasedAlbumIds = new HashSet<int>();
            if (!string.IsNullOrEmpty(userLoginId))
            {
                purchasedAlbumIds = await _purchaseService.GetPurchasedAlbumIds(userLoginId);
            }

            // Crear el ViewModel y pasarlo a la vista
            var viewModel = new StoreIndexViewModel
            {
                AvailableAlbums = albums,
                PurchasedAlbumIds = purchasedAlbumIds
            };
            return View(viewModel);
        }

        // POST: Store/BuyAlbum
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyAlbum(int albumId)
        {
            var userLoginId = HttpContext.Session.GetString("UserName");
            var (success, message) = await _purchaseService.BuyAlbum(albumId, userLoginId);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(MyPurchases));
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Store/MyPurchases (Mostrar las compras del usuario actual)
        public async Task<IActionResult> MyPurchases()
        {
            var userLoginId = HttpContext.Session.GetString("UserLoginId");
            var myPurchases = await _purchaseService.GetUserPurchases(userLoginId);
            return View(myPurchases);
        }
    }
}