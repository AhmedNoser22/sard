namespace Sard.Application.Interfaces.Notification
{
    public interface INotificationService
    {
        Task NotifyLikeAsync(string actorId, string targetUserId, int postId);
        Task NotifyReplyAsync(string actorId, string targetUserId, int postId);
    }
}
