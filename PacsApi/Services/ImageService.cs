using EFCore.lib.Utility;
using Logging;
using PacsApi.Authentication;
using PacsApi.DataManagement;
using PacsApi.DTO;
using LogLevel = Logging.LogLevel;

namespace PacsApi.Services
{
    public class ImageService : Service
    {
        private readonly UserManager _userManager;
        public ImageService(UserManager userManager, LoggerService logger, IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory, logger)
        {
            _userManager = userManager;
        }
        public async Task DeleteAll(string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            using (var unitOfWork = Init(context))
            {
                var dbHandler = new DBHandler(unitOfWork, context);
                await dbHandler.BeginTransactionAsync();

                try
                {
                    var filePaths = await dbHandler.GetAllImagePaths();

                    foreach (var path in filePaths)
                    {
                        try
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, $"Failed to delete file {path}: {ex.Message}");
                        }
                    }

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
            }
        }
        public async Task<List<StudyView>> GetAllStudies(string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            using (var unitOfWork = Init(context))
            {
                var dbHandler = new DBHandler(unitOfWork, context);
                return await dbHandler.GetAllStudyView();
            }
        }

        public async Task<List<ImageView>> GetImages(string studyUid, string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            using (var unitOfWork = Init(context))
            {
                var dbHandler = new DBHandler(unitOfWork, context);
                return await dbHandler.GetImageViews(studyUid);
            }
        }

        public async Task<FileStream?> GetImageFile(string sopUid, string userId)
        {
            var context = _userManager.GetUserDbContext(userId);
            using (var unitOfWork = Init(context))
            {
                var dbHandler = new DBHandler(unitOfWork, context);
                var filePath = await dbHandler.GetImagePath(sopUid);

                if (filePath == null || !File.Exists(filePath))
                    return null;

                return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }
    }
}
