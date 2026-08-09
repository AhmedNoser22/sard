namespace Sard.Infrastructure.Services
{
    public class ImageService(IOptions<CloudinarySettings> options) : IImageService
    {
        private readonly Cloudinary _cloudinary = new(new CloudinaryDotNet.Account(
            options.Value.CloudName,
            options.Value.ApiKey,
            options.Value.ApiSecret));

        public async Task<Result<string>> UploadAsync(IFormFile file, string folder)
        {
            if (file.Length == 0)
                return Result<string>.Failure("الملف فارغ");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation().Width(400).Height(400).Crop("fill").Gravity("face")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                return Result<string>.Failure(result.Error.Message);

            return Result<string>.Success(result.SecureUrl.ToString());
        }

        public async Task DeleteAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var publicId = string.Join("/", imageUrl
                .Split('/')
                .TakeLast(2))
                .Split('.')[0];

            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
    }
}
