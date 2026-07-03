namespace Sard.Application.DTOs.Post
{
    public record PostDto(
    int Id,
    string Content,
    string AuthorId,
    string AuthorName,
    string? AuthorImageUrl,
    int LikesCount,
    int CommentsCount,
    bool IsLikedByMe,
    PostStatus Status,
    DateTime CreatedAt,
    IEnumerable<ReplyDto> Replies
);
}
