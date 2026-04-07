using EFCore.lib.Utility;
using Logging;
using PacsApi.Authentication;
using PacsApi.Context;
using PacsApi.DataManagement;
using PacsApi.DTO;
using LogLevel = Logging.LogLevel;

namespace PacsApi.Services
{
    public class ImageService : Service
    {
        private readonly PacsDbContextFactory _contextFactory;

        public ImageService(
            PacsDbContextFactory contextFactory,
            LoggerService logger,
            IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory, logger)
        {
            _contextFactory = contextFactory;
        }

        public async Task DeleteAll(string userId)
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            using var unitOfWork = Init(context);

            var dbHandler = new DBHandler(unitOfWork, context);

            await dbHandler.BeginTransactionAsync();

            List<string> filePaths = new();

            try
            {
                // Fetch file paths first
                filePaths = await dbHandler.GetAllImagePaths();

                // Delete DB records
                await dbHandler.DeleteAllImages();
                await dbHandler.DeleteAllSeries();
                await dbHandler.DeleteAllStudies();
                await dbHandler.DeleteAllPatients();

                await dbHandler.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbHandler.RollbackAsync();
                Logger.Log(LogLevel.Error, $"Failed to delete all data: {ex.Message}");
                throw;
            }

            // 🔥 Delete files AFTER DB commit (avoids inconsistency)
            foreach (var path in filePaths)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Failed to delete file {path}: {ex.Message}");
                }
            }
        }

        public async Task<List<StudyView>> GetAllStudies(string userId)
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            using var unitOfWork = Init(context);

            var dbHandler = new DBHandler(unitOfWork, context);

            var result = await dbHandler.GetAllStudyView();

            return result ?? new List<StudyView>();
        }

        public async Task<List<ImageView>> GetImages(string studyUid, string userId)
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            using var unitOfWork = Init(context);

            var dbHandler = new DBHandler(unitOfWork, context);

            if (string.IsNullOrWhiteSpace(studyUid))
                return new List<ImageView>();

            var result = await dbHandler.GetImageViews(studyUid);

            return result ?? new List<ImageView>();
        }

        public async Task<FileStream?> GetImageFile(string sopUid, string userId)
        {
            using var context = _contextFactory.CreateDbContext(Array.Empty<string>());
            using var unitOfWork = Init(context);

            var dbHandler = new DBHandler(unitOfWork, context);

            if (string.IsNullOrWhiteSpace(sopUid))
                return null;

            var filePath = await dbHandler.GetImagePath(sopUid);

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                // Open stream safely
                return new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Failed to open file {filePath}: {ex.Message}");
                return null;
            }
        }
    }
}
