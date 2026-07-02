namespace Sard.Application.DTOs.Profile
{
    public class UpdateProfileImageDto
    {
        public IFormFile Image { get; set; } = default!;
    }
}
