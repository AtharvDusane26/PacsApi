using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PacsApi.Services;
using PacsApi.Services.Import;

namespace PacsApi.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly Validator _validator;
        private readonly ImageService _imageService;
        private readonly LoggerService _logger;

        public ImageController(Validator validator, ImageService imageService, LoggerService logger)
        {
            _validator = validator;
            _imageService = imageService;
            _logger = logger;
        }

        [HttpGet("studies")]
        public async Task<IActionResult> GetStudies(string userId)
        {
            try
            {
                _validator.ValidateUser(userId);
                var data = await _imageService.GetAllStudies(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"GetStudies failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{studyUid}")]
        public async Task<IActionResult> GetImages(string studyUid, string userId)
        {
            try
            {
                _validator.ValidateUser(userId);
                var images = await _imageService.GetImages(studyUid, userId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"GetImages failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("file")]
        public async Task<IActionResult> GetImageFile(string sopUid, string userId)
        {
            try
            {
                _validator.ValidateUser(userId);
                var stream = await _imageService.GetImageFile(sopUid, userId);

                if (stream == null)
                    return NotFound();

                return File(stream, "application/dicom", $"{sopUid}.dic");
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"GetImageFile failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("deleteall")]
        public async Task<IActionResult> DeleteAll(string userId)
        {
            try
            {
                _validator.ValidateUser(userId);
                await _imageService.DeleteAll(userId);

                return Ok(new { message = "All data deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.Log(Logging.LogLevel.Error, $"DeleteAll failed: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
