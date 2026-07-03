namespace Sard.Application.DTOs.Notification
{
    public record NotificationDto(
    string Type,
    string Message,
    string ActorName,
    int? PostId,
    DateTime CreatedAt
);
}
