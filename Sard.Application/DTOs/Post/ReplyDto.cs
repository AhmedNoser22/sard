namespace Sard.Application.DTOs.Post
{
    public record ReplyDto(
    int Id,
    string Content,
    string AuthorId,
    string AuthorName,
    string? AuthorImageUrl,
    DateTime CreatedAt
);
}
