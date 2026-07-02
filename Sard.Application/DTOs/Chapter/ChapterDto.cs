namespace Sard.Application.DTOs.Chapter
{
    public record ChapterDto(
     int Id,
     string Title,
     string Content,
     int Order,
     DateTime LastEditedAt
 );
}
