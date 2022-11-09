using Microsoft.AspNetCore.SignalR;

namespace SignalR_Lesson
{
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        
        
        public  async Task SendMessageToAll(string message)
        {
            var name = Context.UserIdentifier;
            var id = Context.ConnectionId;
            await Clients.All.SendAsync("GetMessage", message);
        }

        public async Task SendPrivateMesage(string who, string message)
        {
            Clients.Client(who).SendAsync("GetMessage", message);
        }

        public async Task GetUserConnections()
        {
            var users = _connections.GetConnections("Users");
            await Clients.All.SendAsync("GetUsers", users);

        }

        public override async Task OnConnectedAsync()
        {
            _connections.Add("Users", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }
}
