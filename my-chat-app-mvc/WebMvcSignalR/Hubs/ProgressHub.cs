using Microsoft.AspNetCore.SignalR;

namespace WebMvcSignalR.Hubs
{
    public class ProgressHub : Hub
    {
        public const string GROUP_NAME = "progress";

        public override Task OnConnectedAsync()
        {
            // https://github.com/aspnet/SignalR/issues/2200
            // https://docs.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/working-with-groups
            return Groups.AddToGroupAsync(Context.ConnectionId, "progress");
        }
    }
}
