namespace Sard.Application.DTOs.Group
{
    public record GroupMessageDto(
        int Id,
        string Content,
        string SenderId,
        string SenderName,
        string? SenderImageUrl,
        int GroupId,
        DateTime CreatedAt,
        bool IsDeleted,
        List<MessageReactionSummaryDto> Reactions,
        string? MyReaction
    );
}