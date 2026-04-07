using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PacsApi.Services;
using PacsApi.Services.Import;

namespace PacsApi.Controllers
{
    [ApiController]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly Validator _validator;
        private readonly ImportService _dicomService;
        private readonly LoggerService _logger;

        public ImportController(Validator validator, ImportService dicomService, LoggerService logger)
        {
            _validator = validator;
            _dicomService = dicomService;
            _logger = logger;
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple(string userId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            if (files.Count > GeneralSettings.BatchSize)
                return BadRequest($"Batch size exceeded. Max allowed is {GeneralSettings.BatchSize}");

            try
            {
                _validator.ValidateUser(userId);
                await _dicomService.Upload(userId, files);

                return Ok(new { message = "Files uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"Upload failed for userId: {userId}. Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("batch-total")]
        public IActionResult GetTotalFiles(string userId, string batchGroupId)
        {
            try
            {
                var total = _dicomService.GetTotalFiles(userId, batchGroupId);

                return Ok(new { totalFiles = total });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"GetTotalFiles failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("batch")]
        public IActionResult RemoveBatch(string userId, string batchGroupId)
        {
            try
            {
                _dicomService.RemoveBatch(userId, batchGroupId);

                return Ok(new { message = "Batch removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"RemoveBatch failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
