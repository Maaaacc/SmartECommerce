using SmartECommerce.Interface;

namespace SmartECommerce.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
        private readonly long _defaultMaxFileSize = 2 * 1024 * 1024; // 2 MB

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder, long? maxFileSize = null)
        {
            if(file == null || folder.Length == 0)
            {
                return null;
            }

            // Check file size
            var maxSize = maxFileSize ?? _defaultMaxFileSize;

            if(file.Length > maxSize)
            {
                throw new InvalidOperationException($"File size cannot exceed {maxSize / (1024 * 1024)} MB.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"File type {extension} is not allowed. Only JPG, JPEG, and PNG are allowed.");
            }

            // Generate unique file name
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Create folder if not exists
            var uploadPath = Path.Combine(_env.WebRootPath, folder);

            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public Task DeleteImageAsync(string fileName, string folder)
        {
            if(string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folder))
            {
                return Task.CompletedTask;
            }

            var filePath = Path.Combine(_env.WebRootPath, folder, fileName);

            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
    

}
