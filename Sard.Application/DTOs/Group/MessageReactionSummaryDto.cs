namespace Sard.Application.DTOs.Group
{
    public record MessageReactionSummaryDto(
        string Emoji,
        int Count,
        bool ReactedByMe,
        List<string> ReactorNames
    );
}
