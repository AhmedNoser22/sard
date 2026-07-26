namespace Sard.Application.DTOs.Admin
{
    public record AdminPostDto(
    int Id,
    string Content,
    string AuthorId,
    string AuthorName,
    string? AuthorImageUrl,
    int LikesCount,
    int CommentsCount,
    PostStatus Status,
    DateTime CreatedAt,
    IEnumerable<AdminReplyDto> Replies,
    IEnumerable<string> LikedByNames
);

    public record AdminReplyDto(
        int Id,
        string Content,
        string AuthorId,
        string AuthorName,
        string? AuthorImageUrl,
        DateTime CreatedAt
    );
}
