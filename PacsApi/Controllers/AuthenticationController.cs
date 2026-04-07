using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PacsApi.Services;

namespace PacsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly Validator _validator;
        private readonly LoggerService _logger;

        public AuthenticationController(Validator validator, LoggerService logger)
        {
            _validator = validator;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login(string username, string token)
        {
            try
            {
                var userId = _validator.Login(username, token);

                return Ok(new
                {
                    message = "Login successful",
                    userId
                });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"Login failed for username: {username}. Error: {ex.Message}");
                return Unauthorized(new
                {
                    message = "Login failed",
                    error = ex.Message
                });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout(string userId)
        {
            try
            {
                _validator.Logout(userId);

                return Ok(new
                {
                    message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"Logout failed for userId: {userId}. Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
