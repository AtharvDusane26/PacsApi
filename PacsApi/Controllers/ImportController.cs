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
        private readonly ImportService _importService;
        private readonly LoggerService _logger;

        public ImportController(Validator validator, ImportService dicomService, LoggerService logger)
        {
            _validator = validator;
            _importService = dicomService;
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
                await _importService.Upload(userId, files);

                return Ok(new { message = "Files uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"Upload failed for userId: {userId}. Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
