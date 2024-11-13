using Microsoft.AspNetCore.SignalR.Client;
using MauiChatApp.Interfaces;
using MauiSharedContent.Models;

namespace MauiChatApp.Services
{
    public class ChatService : IChatService
    {
        private HubConnection? m_hubConnection;

        public event Action<string, string, DateTime>? OnMessageReceived;
        public event Action<string, string, DateTime>? OnImageReceived;
        public event Action<string, string>? OnVideoFrameReceived;
        public event Action<List<string>>? OnRoomListReceived;
        public event Action<string>? OnUserKickedFromRoom;
        public event Action<string>? OnRoomDeleted;
        public event Action<string>? OnBannedFromRoom;
        public event Action<List<UserModel>>? OnRoomUsersReceived;

        public async Task ConnectAsync(string p_url)
        {
            m_hubConnection = new HubConnectionBuilder()
                .WithUrl(p_url)
                .Build();

            // Handling the incoming messages, images, video frames from the server
            m_hubConnection.On<string, string, DateTime>("ReceiveMessage", (p_user, p_message, p_timeStamp) =>
            {
                OnMessageReceived?.Invoke(p_user, p_message, p_timeStamp);
            });

            m_hubConnection.On<string, string, DateTime>("ReceiveImage", (p_user, p_base64Image, p_timeStamp) =>
            {
                OnImageReceived?.Invoke(p_user, p_base64Image, p_timeStamp);
            });

            m_hubConnection.On<string, string>("ReceiveVideoFrame", (p_user, p_base64Image) =>
            {
                OnVideoFrameReceived?.Invoke(p_user, p_base64Image);
            });

            m_hubConnection.On<string>("UserKickedFromRoom", (roomName) =>
            {
                OnUserKickedFromRoom?.Invoke(roomName);
            });

            // Get the list of rooms from the server
            m_hubConnection.On<List<string>>("ReceiveRoomList", (p_rooms) =>
            {
                OnRoomListReceived?.Invoke(p_rooms);
            });

            m_hubConnection.On<string>("RoomDeleted", (p_roomName) =>
            {
                OnRoomDeleted?.Invoke(p_roomName);
            });

            m_hubConnection.On<string>("BannedFromRoom", (room) =>
            {
                OnBannedFromRoom?.Invoke(room);
            });

            m_hubConnection.On<List<string>>("ReceiveRoomUsers", (p_users) =>
            {
                var users = p_users.Select(user => new UserModel { Name = user }).ToList();
                OnRoomUsersReceived?.Invoke(users);
            });

            await m_hubConnection.StartAsync();
        }

        // with the following methods we can send messages, images, video frames to the server, join and leave rooms
        public Task SendMessageAsync(string p_user, string p_room, string p_message)
        {
            return m_hubConnection?.InvokeAsync("SendMessage", p_user, p_room, p_message) ?? Task.CompletedTask;
        }

        public Task SendImageAsync(string p_user, string p_room, string p_base64Image)
        {
            return m_hubConnection?.InvokeAsync("SendImage", p_user, p_room, p_base64Image) ?? Task.CompletedTask;
        }

        public Task SendVideoFrameAsync(string p_user, string p_room, string p_base64Image)
        {
            return m_hubConnection?.InvokeAsync("SendVideoFrame", p_user, p_room, p_base64Image) ?? Task.CompletedTask;
        }

        public Task JoinRoomAsync(string p_user, string p_room)
        {
            return m_hubConnection?.InvokeAsync("JoinRoom", p_user, p_room) ?? Task.CompletedTask;
        }

        public Task LeaveRoomAsync(string p_user, string p_room)
        {
            return m_hubConnection?.InvokeAsync("LeaveRoom", p_user, p_room) ?? Task.CompletedTask;
        }

        public async Task CreateRoomAsync(string p_room)
        {
            await m_hubConnection.SendAsync("CreateRoom", p_room);
        }

        public Task GetRoomUsersAsync(string p_roomName)
        {
            return m_hubConnection?.InvokeAsync("GetRoomUsers", p_roomName) ?? Task.CompletedTask;
        }

        public Task LoadAllMessagesForRoom(string p_selectedRoom)
        {
            return m_hubConnection?.InvokeAsync("LoadAllMessagesForRoom", p_selectedRoom) ?? Task.CompletedTask;
        }
    }
}

