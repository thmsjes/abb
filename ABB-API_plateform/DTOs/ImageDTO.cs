namespace Abb.DTOs
{
    public class ImageDTO
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string Year { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageName { get; set; } = string.Empty;  // ⭐ Changed from FileName
        public string FilePath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }  // ⭐ Changed from UploadedAt
        public int? EventId { get; set; }
    }

    public class ImageUploadResponseDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ImageId { get; set; }
    }
}
