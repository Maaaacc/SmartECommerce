namespace SmartECommerce.Interface
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder, long? maxFileSize = null);
        Task DeleteImageAsync(string fileName, string folder);
    }
}
