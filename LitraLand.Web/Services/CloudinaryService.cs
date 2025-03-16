using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace LitraLand.Web.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly List<string> _allowedImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 2 * 1024 * 1024; // 2097152 bytes (2MB)
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinary)
        {
            Account account = new Account
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(bool isUploaded, string? imageUrl, string? thumbnailUrl, string? publicId, string? errorMessage)>
            UploadImageAsync(IFormFile image, bool hasThumbnail = true)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return (false, null, null, null, "Image is empty");

                // check if the image extension is allowed
                var imageExtension = Path.GetExtension(image.FileName);
                if (!_allowedImageExtensions.Contains(imageExtension))
                    return (false, null, null, null, Errors.NotAllowedExtension);

                // check if the image size is greater than 2MB
                if (image.Length > MaxImageSize)
                    return (false, null, null, null, Errors.MaxSize);

                using var stream = image.OpenReadStream();
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageName, stream),
                    UseFilename = true // to use the same file name as the uploaded file name
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    return (false, null, null, null, uploadResult.Error.Message);

                string imageUrl = uploadResult.SecureUrl.ToString();
                string publicId = uploadResult.PublicId;
                string? thumbnailUrl = hasThumbnail ? GetThumbnailImageUrl(imageUrl) : null;

                return (true, imageUrl, thumbnailUrl, publicId, null);
            }
            catch (Exception ex)
            {
                return (
                    isUploaded: false,
                    imageUrl: null,
                    thumbnailUrl: null,
                    publicId: null,
                    errorMessage: ex.Message);
            }
        }

        public async Task<(bool isDeleted, string? errorMessage)> DeleteImageAsync(string? publicId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicId))
                    return (false, "Invalid public ID.");

                var deleteParams = new DeletionParams(publicId);
                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                if (deleteResult.Error != null)
                    return (false, $"Cloudinary Error: {deleteResult.Error.Message}");

                if (deleteResult.Result == "ok")
                    return (true, null);

                return (false, $"Failed to delete image: {deleteResult.Result}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

        private string GetThumbnailImageUrl(string imageUrl)
        {
            var transformation = "c_thumb,w_200,g_face/";
            var separator = "image/upload/";
            var urlParts = imageUrl.Split(separator);

            var thumbnailImageUrl = $"{urlParts[0]}{separator}{transformation}{urlParts[1]}";

            return thumbnailImageUrl;
        }
    }
}