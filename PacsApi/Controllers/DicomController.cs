using Microsoft.AspNetCore.Mvc;
using PacsApi.Services;
namespace PacsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DicomController : ControllerBase
    {
        private readonly Manager _manager;
        private readonly DicomService _dicomService;

        public DicomController(Manager manager, DicomService dicomService)
        {
            _manager = manager;
            _dicomService = dicomService;
        }

        // ================= LOGIN =================

        [HttpPost("login")]
        public IActionResult Login(string username, string token)
        {
            try
            {
                var userId = _manager.Login(username, token);

                return Ok(new
                {
                    message = "Login successful",
                    userId
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    message = "Login failed",
                    error = ex.Message
                });
            }
        }

        // ================= LOGOUT =================

        [HttpPost("logout")]
        public IActionResult Logout(string userId)
        {
            try
            {
                _manager.Logout(userId);

                return Ok(new
                {
                    message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ================= UPLOAD =================
        // Phase 1 → Only store in batches

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple(string userId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");
            if (files.Count > GeneralSettings.BatchSize)
                return BadRequest($"Batch size exceeded. Max allowed is {GeneralSettings.BatchSize}");
            try
            {
                await _manager.Upload(userId, files,_dicomService);

                return Ok(new
                {
                    message = "Files uploaded successfully",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        // ================= OPTIONAL HELPERS =================

        [HttpGet("batch-total")]
        public IActionResult GetTotalFiles(string userId, string batchGroupId)
        {
            try
            {
                var total = _manager.GetTotalFiles(userId, batchGroupId);

                return Ok(new
                {
                    totalFiles = total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("batch")]
        public IActionResult RemoveBatch(string userId, string batchGroupId)
        {
            try
            {
                _manager.RemoveBatch(userId, batchGroupId);

                return Ok(new
                {
                    message = "Batch removed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ================= EXISTING DATA APIs =================

        [HttpGet("studies")]
        public async Task<IActionResult> GetStudies(string userId)
        {
            try
            {
                var data = await _dicomService.GetAllStudies(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("images/{studyUid}")]
        public async Task<IActionResult> GetImages(string studyUid, string userId)
        {
            try
            {
                var images = await _dicomService.GetImages(studyUid, userId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("image/file")]
        public async Task<IActionResult> GetImageFile(string sopUid, string userId)
        {
            try
            {
                var stream = await _dicomService.GetImageFile(sopUid, userId);

                if (stream == null)
                    return NotFound();

                return File(stream, "application/dicom", $"{sopUid}.dic");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("deleteall")]
        public async Task<IActionResult> DeleteAll(string userId)
        {
            try
            {
                await _dicomService.DeleteAll(userId);

                return Ok(new
                {
                    message = "All data deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
