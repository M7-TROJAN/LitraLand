namespace LitraLand.Web.Services
{
    public class ImageService : IImageServices
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly List<string> _allowedImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 2 * 1024 * 1024; // 2097152 bytes (2MB)
        public ImageService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        {
            // get the image extension
            var imageExtension = Path.GetExtension(image.FileName);

            // check if the image extension is allowed
            if (!_allowedImageExtensions.Contains(imageExtension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);

            // check if the image size is greater than 2MB
            if (image.Length > MaxImageSize)
                return (isUploaded: false, errorMessage: Errors.MaxSize);

            // check if the imageName ends with the image extension (example: imageName.jpg)
            if (!imageName.EndsWith(imageExtension))
                imageName += imageExtension;

            // generate the path for the image and the thumbnail
            var path = Path.Combine($"{_hostingEnvironment.WebRootPath}{folderPath}", imageName); // for example: wwwroot/images/books/imageName
            var thumbPath = Path.Combine($"{_hostingEnvironment.WebRootPath}{folderPath}/thumb", imageName); // for example: wwwroot/images/books/thumb/imageName 

            // start uploading the image to the server
            using var stream = File.Create(path); // create the image file
            await image.CopyToAsync(stream); // copy the image to the file
            stream.Dispose(); // close the stream

            // create the thumbnail if the hasThumbnail is true
            if (hasThumbnail)
                CreateThumbnail(image, thumbPath); // create the thumbnail

            return (isUploaded: true, errorMessage: null);
        }

        public void Delete(string imagePath, string? imageThumbnailPath = null)
        {
            var oldImagePath = $"{_hostingEnvironment.WebRootPath}{imagePath}";
            if (File.Exists(oldImagePath))
                File.Delete(oldImagePath);

            if (!string.IsNullOrEmpty(imageThumbnailPath))
            {
                var oldThumbPath = $"{_hostingEnvironment.WebRootPath}{imageThumbnailPath}";

                if (File.Exists(oldThumbPath))
                    File.Delete(oldThumbPath);
            }
        }

        public bool IsAllowedImageExtension(IFormFile image, string[] allowedExtensions)
        {
            if (image is null)
                return false;

            var imageExtension = Path.GetExtension(image.FileName);
            return allowedExtensions.Contains(imageExtension);
        }

        private void CreateThumbnail(IFormFile image, string thumbPath)
        {
            // use imageSharp to create the thumbnail
            using var loadedImage = Image.Load(image.OpenReadStream()); // // use ImageSharp package to load the image
            var newWidth = 200; // the new width of the thumbnail image
            var ratio = (float)loadedImage.Width / newWidth; // calculate the ratio (ratio means the width of the image divided by the new width you want)
            var height = loadedImage.Height / ratio; // calculate the new height based on the ratio (example: if the image width is 400 and the new width is 200, then the ratio is 400/200 = 2, then the new height is height/2)
            loadedImage.Mutate(i => i.Resize(width: newWidth, height: (int)height)); // resize the image
            loadedImage.Save(thumbPath); // save the thumbnail image
            loadedImage.Dispose(); // dispose the image
        }
    }
}