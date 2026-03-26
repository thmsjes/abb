using ABB_API_plateform.Business;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ABB_API_plateform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowReact")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _service;  // ⭐ Use interface
        private readonly ILogging _logging;

        public ImagesController(IImageService service, ILogging logging)  // ⭐ Use interface
        {
            _logging = logging;
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(int propertyId, string year, string category, int? eventId, IFormFile file)
        {
            try
            {
                _logging.LogToFile($"Image upload - PropertyId: {propertyId}, Year: {year}, Category: {category}, EventId: {eventId}, FileName: {file?.FileName}");

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { IsSuccess = false, Message = "No file provided" });
                }

                var result = await _service.UploadImageAsync(propertyId, year, category, eventId, file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Image upload exception: {ex.Message}");
                return StatusCode(500, new { IsSuccess = false, Message = $"Error uploading image: {ex.Message}" });
            }
        }

        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetByProperty(int propertyId)
        {
            try
            {
                _logging.LogToFile($"GetByProperty called - PropertyId: {propertyId}");
                var result = await _service.GetByPropertyAsync(propertyId);
                _logging.LogToFile($"GetByProperty completed - Found {result.Count} images");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"EXCEPTION in GetByProperty - PropertyId: {propertyId}, Error: {ex.GetType().Name}, Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { IsSuccess = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(int propertyId, string? year, string? category)
        {
            try
            {
                _logging.LogToFile($"GetFiltered called - PropertyId: {propertyId}, Year: {year}, Category: {category}");
                return Ok(await _service.GetFilteredAsync(propertyId, year, category));
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"EXCEPTION in GetFiltered: {ex.Message}");
                return StatusCode(500, new { IsSuccess = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            var img = await _service.GetByIdAsync(id);
            if (img == null) return NotFound();
            if (!System.IO.File.Exists(img.FilePath)) return NotFound();

            string extension = Path.GetExtension(img.FilePath).ToLower();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            var stream = new FileStream(img.FilePath, FileMode.Open, FileAccess.Read);
            return File(stream, contentType);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logging.LogToFile($"Image delete - Id: {id}");
                await _service.DeleteImageAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Image delete exception: {ex.Message}");
                return StatusCode(500, new { IsSuccess = false, Message = $"Error deleting image: {ex.Message}" });
            }
        }
    }
}