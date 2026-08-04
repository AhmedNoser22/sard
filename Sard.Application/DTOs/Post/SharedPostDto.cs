namespace Sard.Application.DTOs.Post
{
    public record SharedPostDto(
    int ShareId,
    int PostId,
    string Content,
    string AuthorName,
    string? AuthorImageUrl,
    int LikesCount,
    int CommentsCount,
    DateTime OriginalCreatedAt,
    DateTime SharedAt
);
}
