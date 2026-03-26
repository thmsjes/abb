using Abb.DTOs;
using ABB_API_plateform.Data;

namespace ABB_API_plateform.Business
{
    // ⭐ ADD INTERFACE
    public interface IImageService
    {
        Task<ImageUploadResponseDTO> UploadImageAsync(int propertyId, string year, string category, int? eventId, IFormFile file);
        Task<List<ImageDTO>> GetByPropertyAsync(int propertyId);
        Task<List<ImageDTO>> GetFilteredAsync(int propertyId, string? year, string? category);
        Task<ImageDTO?> GetByIdAsync(int id);
        Task DeleteImageAsync(int id);
    }

    public class ImageService : IImageService  // ⭐ Implement interface
    {
        private readonly IImagesClass _repo;  // ⭐ Use interface
        private readonly string _uploadPath;
        private readonly ILogging _logging;

        public ImageService(IImagesClass repo, IConfiguration config, ILogging logging)  // ⭐ Use interface
        {
            _repo = repo;
            _logging = logging;
            _uploadPath = config["ImageUploadPath"] 
                ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<ImageUploadResponseDTO> UploadImageAsync(int propertyId, string year, string category, int? eventId, IFormFile file)
        {
            try
            {
                var dirPath = Path.Combine(_uploadPath, propertyId.ToString(), year, category);
                Directory.CreateDirectory(dirPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(dirPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageDto = new ImageDTO
                {
                    PropertyId = propertyId,
                    Year = year,
                    Category = category,
                    ImageName = fileName,
                    FilePath = filePath,
                    CreatedDate = DateTime.Now,
                    EventId = eventId
                };

                int id = await _repo.InsertAsync(imageDto);

                _logging.LogToFile($"Image uploaded successfully - Id: {id}, PropertyId: {propertyId}, EventId: {eventId}");

                return new ImageUploadResponseDTO
                {
                    IsSuccess = true,
                    Message = "Image uploaded successfully",
                    ImageId = id
                };
            }
            catch (Exception ex)
            {
                _logging.LogToFile($"Error uploading image: {ex.Message}");
                return new ImageUploadResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error uploading image: {ex.Message}"
                };
            }
        }

        public async Task<List<ImageDTO>> GetByPropertyAsync(int propertyId) =>
            await _repo.GetByPropertyIdAsync(propertyId);

        public async Task<List<ImageDTO>> GetFilteredAsync(int propertyId, string? year, string? category) =>
            await _repo.GetFilteredAsync(propertyId, year, category);

        public async Task<ImageDTO?> GetByIdAsync(int id) =>
            await _repo.GetByIdAsync(id);

        public async Task DeleteImageAsync(int id)
        {
            var img = await _repo.GetByIdAsync(id);
            if (img != null && File.Exists(img.FilePath))
            {
                File.Delete(img.FilePath);
            }
            await _repo.DeleteAsync(id);
        }
    }
}