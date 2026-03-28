using PacsApi.Context;
using System.Globalization;

namespace PacsApi.Authentication
{
    public class UserManager
    {
        private readonly List<User> _users;
        private readonly Dictionary<User, PacsDbContext> _userDbContexts;
        private readonly PacsDbContextFactory _pacsDbContextFactory;
        public UserManager(PacsDbContextFactory pacsDbContextFactory)
        {
            _pacsDbContextFactory = pacsDbContextFactory;
            _users = new List<User>();
            _userDbContexts = new Dictionary<User, PacsDbContext>();
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
                _userDbContexts[user] = _pacsDbContextFactory.CreateDbContext(Array.Empty<string>());
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }
        public string StartUserSession(string name, string token)
        {
            var user = _users.FirstOrDefault(u => u.Username == name);
            if (user != null)
            {
                user.StartSession(token);
                return user.Id;
            }
            else
            {
                RegisterValidateUser(name, token);
                StartUserSession(name, token);
            }
            return "";
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
                user = null;
            }
            else
            {
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
                    return _userDbContexts[user];
                }
                else
                {
                    throw new UnauthorizedAccessException("Session is not active");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public void RemoveUser(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.EndSession();
                _userDbContexts[user]?.Dispose();
                _userDbContexts.Remove(user);
                _users.Remove(user);
            }
            else
            {
                throw new UnauthorizedAccessException("User not found");
            }
        }
        public void RemoveAllUsers()
        {
            foreach (var user in _users)
            {
                user.EndSession();
                _userDbContexts[user]?.Dispose();
            }
            _userDbContexts.Clear();
            _users.Clear();
        }
    }
}
