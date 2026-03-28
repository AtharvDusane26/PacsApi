using Logging;
using PacsApi.Authentication;
using PacsApi.DataBank;
using PacsApi.DTO;

namespace PacsApi.Services
{
    public class Manager
    {
        private readonly UserManager _userManager;
        private readonly BatchManager _batchManager;
        private readonly LoggerService _logger;

        public Manager(UserManager userManager, BatchManager batchManager, LoggerService logger)
        {
            _userManager = userManager;
            _batchManager = batchManager;
            _logger = logger;
        }

        // ================= LOGIN =================

        public string Login(string username, string token)
        {
            return _userManager.StartUserSession(username, token);
        }

        public void Logout(string userId)
        {
            _userManager.EndUserSession(userId);
        }

        // ================= VALIDATION =================

        private void ValidateUser(string userId)
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
                    _userManager.StartUserSession(user.Username, "PACSAPI2026TEST");
            }
        }

        // ================= UPLOAD =================

        public async Task Upload(string userId, List<IFormFile> files, DicomService dicomService)
        {
            ValidateUser(userId);

            if (files == null || files.Count == 0)
            {
                _logger.Log(Logging.LogLevel.Error, $"User {userId} attempted to upload with no files.");
                throw new Exception("No files uploaded");
            }

            // 🔥 returns batchGroupId
             await _batchManager.CreateBatch(files, userId, dicomService);
        }

        // ================= PROCESS =================


        // ================= OPTIONAL HELPERS =================

        public int GetTotalFiles(string userId, string batchGroupId)
        {
            ValidateUser(userId);
            return _batchManager.GetTotalFiles(userId, batchGroupId);
        }

        public void RemoveBatch(string userId, string batchGroupId)
        {
            ValidateUser(userId);
            _batchManager.RemoveBatchGroup(userId, batchGroupId);
        }
    }
}
