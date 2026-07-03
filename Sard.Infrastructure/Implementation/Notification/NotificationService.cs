namespace Sard.Infrastructure.Implementation.Notification
{
    public class NotificationService(
    IHubContext<NabdHub> hubContext,
    UserManager<AppUser> userManager) : INotificationService
    {
        public async Task NotifyLikeAsync(string actorId, string targetUserId, int postId)
        {
            if (actorId == targetUserId) return;

            var actor = await userManager.FindByIdAsync(actorId);
            if (actor is null) return;

            var notification = new NotificationDto(
                Type: "like",
                Message: "أعجب بمنشورك",
                ActorName: actor.DisplayName,
                PostId: postId,
                CreatedAt: EgyptDateTime.Now
            );

            await hubContext.Clients
                .Group($"user-{targetUserId}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyReplyAsync(string actorId, string targetUserId, int postId)
        {
            if (actorId == targetUserId) return;

            var actor = await userManager.FindByIdAsync(actorId);
            if (actor is null) return;

            var notification = new NotificationDto(
                Type: "reply",
                Message: "رد على منشورك",
                ActorName: actor.DisplayName,
                PostId: postId,
                CreatedAt: EgyptDateTime.Now
            );

            await hubContext.Clients
                .Group($"user-{targetUserId}")
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
