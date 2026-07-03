namespace Sard.Infrastructure.Hubs;

[Authorize]
public class NabdHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }

    public async Task JoinPost(int postId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"post-{postId}");
    }

    public async Task LeavePost(int postId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"post-{postId}");
    }
}