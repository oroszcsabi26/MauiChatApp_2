using MauiSharedContent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Interfaces
{
    public interface IChatService
    {
        //events from the server
        event Action<string, string> OnVideoFrameReceived;
        event Action<string, string, DateTime> OnMessageReceived;
        event Action<string, string, DateTime> OnImageReceived;
        event Action<List<string>> OnRoomListReceived;
        event Action<string>? OnUserKickedFromRoom;
        event Action<string>? OnRoomDeleted;
        event Action<string>? OnBannedFromRoom;
        event Action<List<UserModel>> OnRoomUsersReceived;

        //messages to the server
        Task SendVideoFrameAsync(string user, string room, string base64Image);
 
        Task ConnectAsync(string url);

        Task SendMessageAsync(string user, string room, string message);

        Task SendImageAsync(string user, string room, string base64Image);

        Task JoinRoomAsync(string user, string room);

        Task LeaveRoomAsync(string user, string room);

        Task CreateRoomAsync(string room);

        Task GetRoomUsersAsync(string roomName);

        Task LoadAllMessagesForRoom(string selectedRoom);
    }
}
