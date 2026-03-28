using PacsApi.Context;

namespace PacsApi.Authentication
{
    public class User
    {
        private string _id;
        private string _username;
        private readonly string _token = "PACSAPI2026TEST";
        private readonly TimeSpan _sessionDuration = TimeSpan.FromMinutes(20);
        private bool _isSessionActive;
        public Action SessionTimeOut;
        private System.Timers.Timer _sessionTimer;
        public User(string name)
        {
            _id = Guid.NewGuid().ToString();
            _username = name;
        }
        public string Id => _id;
        public string Username => _username;
        public bool IsSessionActive => _isSessionActive;
        private void StartSession()
        {
            _sessionTimer = new System.Timers.Timer(_sessionDuration.TotalMilliseconds);
            _sessionTimer.Elapsed += (sender, e) =>
            {
                SessionTimeOut?.Invoke();
                _sessionTimer?.Stop();
                _sessionTimer?.Dispose();
                _isSessionActive = false;
            };
            _sessionTimer.Start();
            _isSessionActive = true;
        }
        public bool ValidateToken(string token)
        {
            return token == _token;
        }
        public void EndSession()
        {
            _sessionTimer?.Stop();
            _sessionTimer?.Dispose();
            _isSessionActive = false;
        }
        public void StartSession(string token)
        {
            if (ValidateToken(token))
            {
                StartSession();
            }
            else
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }
    }
}
