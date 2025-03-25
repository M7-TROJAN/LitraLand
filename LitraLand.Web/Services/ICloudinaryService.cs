namespace LitraLand.Web.Services
{
    public interface ICloudinaryService
    {
        Task<(bool isUploaded, string? imageUrl, string? thumbnailUrl, string? publicId, string? errorMessage)>
            UploadImageAsync(IFormFile image, bool hasThumbnail = true);

        Task<(bool isDeleted, string? errorMessage)> DeleteImageAsync(string? publicId);
    }
}