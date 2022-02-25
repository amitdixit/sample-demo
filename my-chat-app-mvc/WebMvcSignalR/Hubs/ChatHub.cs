using ModelLibrary.Models;
using Microsoft.AspNetCore.SignalR;

namespace WebMvcSignalR.Hubs
{
   
    public class ChatHub : Hub
    {

        private readonly static ConnectionMapping<string> _connections = new ConnectionMapping<string>();


        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}

        public async Task SendMessage(string message)
        {
            string? user = Context?.User?.Identity?.Name;
            await Clients.All.SendAsync("ReceiveMessage", user, message, Context.ConnectionId);
        }

        public Task SendMessageToCaller(string user, string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", user, message);
        }

        public Task SendMessageToGroup(string user, string message)
        {
            return Clients.Group("SignalR Users").SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            string? name = Context?.User?.Identity?.Name;

            _connections.Add(name, Context.ConnectionId);

            await Clients.All.SendAsync("UserLoggedIn", name, Context.ConnectionId);//$"{name} has joined"
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string name = Context.User.Identity.Name;

            _connections.Remove(name, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToSelectedUser(string connectionId,string message) => await Clients.Clients(new List<string> { connectionId, Context.ConnectionId }).SendAsync("MyMessageReceived", Context?.User?.Identity?.Name, message , Context.ConnectionId);
        public string GetConnectionId() => Context.ConnectionId;

        public dynamic GetAllUsers() => _connections.GetAllConnections();
    }
}
