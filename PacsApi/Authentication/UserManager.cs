using Logging;
using PacsApi.Context;
using System.Globalization;

namespace PacsApi.Authentication
{
    public class UserManager
    {
        private readonly List<User> _users;
        private readonly Dictionary<User, PacsDbContextFactory> _userDbContexts;
        private readonly PacsDbContextFactory _pacsDbContextFactory;
        private readonly LoggerService _logger;
        public UserManager(PacsDbContextFactory pacsDbContextFactory, LoggerService logger)
        {
            _pacsDbContextFactory = pacsDbContextFactory;
            _users = new List<User>();
            _userDbContexts = new Dictionary<User, PacsDbContextFactory>();
            _logger = logger;
        }
        private void RegisterValidateUser(string name, string token)
        {
            var user = new User(name);
            user.SessionTimeOut += () =>
            {
                RemoveUser(user.Id);
            };
            if (user.ValidateToken(token))
            {
                _users.Add(user);
                _userDbContexts[user] = _pacsDbContextFactory;
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Failed login attempt for user {name} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("Invalid token");
            }
        }
        public string StartUserSession(string name, string token)
        {
            var user = _users.FirstOrDefault(u => u.Username == name && !u.IsSessionActive);
            if (user != null && !user.IsSessionActive)
            {
                user.StartSession(token);
                _logger.Log(Logging.LogLevel.Info, $"{name} session started at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                return user.Id;
            }
            else
            {
                RegisterValidateUser(name, token);
                return StartUserSession(name, token);
            }
        }
        public User GetUser(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                return user;
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Attempt to access user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public void EndUserSession(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                GetUserDbContext(user.Id)?.Dispose();
                _userDbContexts.Remove(user);
                _users.Remove(user);
                user.EndSession();
                _logger.Log(Logging.LogLevel.Info, $"{user.Username} session ended at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                user = null;
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Attempt to end session for user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public bool ValidateUserSession(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                if (!user.IsSessionActive)
                {
                    return false;
                }
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Attempt to validate session for user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("User not found");
            }
            return true;
        }
        public PacsDbContext GetUserDbContext(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                if (user.IsSessionActive)
                {
                    return _userDbContexts[user].CreateDbContext(Array.Empty<string>());
                }
                else
                {
                    _logger.Log(Logging.LogLevel.Error, $"Attempt to access DB context for inactive session of user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                    throw new UnauthorizedAccessException("Session is not active");
                }
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Attempt to access DB context for user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public void RemoveUser(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.EndSession();
                _userDbContexts.Remove(user);
                _users.Remove(user);
                _logger.Log(Logging.LogLevel.Info, $"User with id {id} removed at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
            }
            else
            {
                _logger.Log(Logging.LogLevel.Error, $"Attempt to remove user with id {id} at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public void RemoveAllUsers()
        {
            foreach (var user in _users)
            {
                user.EndSession();
            }
            _userDbContexts.Clear();
            _users.Clear();
            _logger.Log(Logging.LogLevel.Info, $"All users removed at {DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
        }
    }
}
