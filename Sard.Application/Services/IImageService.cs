namespace Sard.Application.Services
{
    public interface IImageService
    {
        Task<Result<string>> UploadAsync(IFormFile file, string folder);
        Task DeleteAsync(string imageUrl);
    }
}
