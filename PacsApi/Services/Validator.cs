using Logging;
using PacsApi.Authentication;
using PacsApi.DTO;
using PacsApi.Services.Import;

namespace PacsApi.Services
{
    public class Validator
    {
        private readonly UserManager _userManager;
        private readonly LoggerService _logger;

        public Validator(UserManager userManager, LoggerService logger)
        {
            _userManager = userManager;
            _logger = logger;
        }
        public string Login(string username, string token)
        {
            return _userManager.StartUserSession(username, token);
        }

        public void Logout(string userId)
        {
            _userManager.EndUserSession(userId);
        }


        public void ValidateUser(string userId)
        {
            if (!_userManager.ValidateUserSession(userId))
            {
                var user = _userManager.GetUser(userId);
                if (user == null)
                {
                    _logger.Log(Logging.LogLevel.Error, $"Unauthorized access attempt with userId: {userId}");
                    throw new UnauthorizedAccessException("User not found");
                }
                else
                    _userManager.StartUserSession(user.Username, GeneralSettings.ApiToken);
            }
        }
    }
}
