using MusicRadioInc.Models;

namespace MusicRadioInc.Interfaces
{
    public interface IAuthService
    {
        Task<Client> ValidateUserCredentials(LoginViewModel model);
        Task<bool> RegisterNewUser(Client user);
        Task<bool> IsUserLoginIdTaken(string userLoginId);
    }
}
