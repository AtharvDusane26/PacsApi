using Logging;
using PacsApi.Context;
using PacsApi.DTO;
using PacsApi.Services;
using System.Collections.Concurrent;

namespace PacsApi.DataBank
{
    public class BatchManager
    {
        // userId → lock (ensures sequential processing per user)
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks
            = new();
        private readonly LoggerService _logger;

        public BatchManager(LoggerService logger)
        {
            _logger = logger;
        }
        // ================= CREATE =================

        public async Task<string> CreateBatch(List<IFormFile> files, string userId, DicomService dicomService)
        {
            var userLock = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

            var batch = new Batch();
            batch.Create(files);
            batch.SetOwner(userId);

            await userLock.WaitAsync();

            try
            {
                foreach (var file in batch.GetBuckets())
                {
                    try
                    {
                        await dicomService.ProcessRawDicomStreamAsync(
                            file.GetStream(),
                            userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(Logging.LogLevel.Error, $"Error processing DICOM stream for user {userId}: {ex.Message}");
                    }
                }
            }
            finally
            {
                userLock.Release();
                batch.Dispose();
            }

            // dummy id (for compatibility)
            return Guid.NewGuid().ToString();
        }

        // ================= GET =================

        public ConcurrentQueue<Batch> GetBatches(string userId, string batchGroupId)
        {
            // no storage anymore → return empty
            return new ConcurrentQueue<Batch>();
        }

        // ================= PROCESS =================

        public async Task<UploadProgress> ProcessNextBatch(
            string userId,
            string batchGroupId,
            DicomService dicomService)
        {
            // no-op (already processed in CreateBatch)
            return new UploadProgress
            {
                Processed = 0,
                Total = 0,
                Percentage = 100
            };
        }

        // ================= TOTAL =================

        public int GetTotalFiles(string userId, string batchGroupId)
        {
            return 0; // no tracking
        }

        // ================= CLEAN =================

        public void RemoveBatchGroup(string userId, string batchGroupId)
        {
            _userLocks.TryRemove(userId, out _);
        }
    }
}
