using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiAdminApp.Interfaces
{
    public interface IAdminChatService
    {
        event Action<List<string>> OnRoomListReceived;
        event Action<List<string>> OnRoomUsersReceived;
        event Action<string> OnUserKicked;
        event Action<string, string, DateTime>? OnMessageReceived;
        public event Action<string, string, DateTime>? OnImageReceived;

        Task ConnectAsync(string serverUrl);

        Task AdminCreateRoomAsync(string roomName);

        Task AdminDeleteRoomAsync(string roomName);

        Task KickUserAsync(string roomName, string userName);

        Task GetRoomListAsync();

        Task GetRoomUsersAsync(string roomName);

        Task AdminRefreshMessagesForRoom(string roomName);

        Task LoadAllMessagesForRoom(string roomName);
    }
}
