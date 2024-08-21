using ChatWebApp.Data;
using ChatWebApp.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatWebApp.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            Client client = new Client

            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };

            ClientSource.Clients.Add(client);
            await Clients.Others.SendAsync("clientJoined", nickName);
            await Clients.All.SendAsync("clients", ClientSource.Clients);
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if (client != null)
            {
                ClientSource.Clients.Remove(client);
                await Clients.All.SendAsync("clientLeft", client.NickName);
                await Clients.All.SendAsync("clients", ClientSource.Clients);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task GetClients()
        {
            var clients = ClientSource.Clients;
            await Clients.All.SendAsync("GetClients", clients);
        }

        public async Task SendMessageAsync(string message, string clientName)
        {
            clientName = clientName.Trim();
            Client senderClient = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            if (clientName == "Mesajını Tüm Sohbetlere Gönder")
            {
                await Clients.All.SendAsync("receiveMessage", message, senderClient.NickName);
            }
            else
            {
                Client client = ClientSource.Clients.FirstOrDefault(c => c.NickName == clientName);
                await Clients.Client(client.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);
                await Clients.Client(senderClient.ConnectionId).SendAsync("receiveMessage", message, senderClient.NickName);


            }
        }



        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            Group group = new Group { GroupName = groupName };
            group.Clients.Add(ClientSource.Clients.FirstOrDefault( c => c.ConnectionId == Context.ConnectionId));
                
            GroupSource.Groups.Add(group);

            await Clients.All.SendAsync("groups", GroupSource.Groups);
        }

        public async Task AddClientToGroup(IEnumerable<string> groupNames)
        {
            Client client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);

            foreach (var group in groupNames)
            {
                Group _group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == group);

                var result = _group.Clients.Any(c => c.ConnectionId == Context.ConnectionId);
                if (!result)
                {
                    _group.Clients.Add(client);
                    await Groups.AddToGroupAsync(Context.ConnectionId, group);
                }
            }
        }


        public async Task GetGroupMembers(string groupName)
        {
            var group = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);
            if (group != null)
            {
                var members = group.Clients.Select(c => c.NickName).ToList();
                await Clients.Caller.SendAsync("receiveGroupMembers", groupName, members);
            }
        }

        public async Task GetClientToGroup(string groupName)
        
         {
            Group group = GroupSource.Groups.FirstOrDefault( g => g.GroupName == groupName);
            await Clients.Caller.SendAsync("clients", groupName == "-1" ? ClientSource.Clients : group.Clients);
        }

        public async Task SendMessageToGroupAsync(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("receiveGroupMessage", groupName, message, ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId).NickName);
        }


    }

}
