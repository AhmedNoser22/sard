namespace Sard.Application.DTOs.Novel
{
    public record SaveChapterDto(
    string Title,
    string Content,
    int Order
);
}
