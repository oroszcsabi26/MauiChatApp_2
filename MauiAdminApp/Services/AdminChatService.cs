using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MauiAdminApp.Interfaces;

namespace MauiAdminApp.Services
{
    public class AdminChatService : IAdminChatService
    {
        private HubConnection? _hubConnection;

        public event Action<List<string>>? OnRoomListReceived;
        public event Action<List<string>>? OnRoomUsersReceived;
        public event Action<string>? OnUserKicked;
        public event Action<string, string, DateTime>? OnMessageReceived;
        public event Action<string, string, DateTime>? OnImageReceived;

        public async Task ConnectAsync(string p_serverUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(p_serverUrl)
                .Build();

            _hubConnection.On<List<string>>("ReceiveRoomList", p_rooms =>
            {
                OnRoomListReceived?.Invoke(p_rooms);
            });

            _hubConnection.On<List<string>>("ReceiveRoomUsers", p_users =>
            {
                OnRoomUsersReceived?.Invoke(p_users);
            });

            _hubConnection.On<string>("UserKicked", p_user =>
            {
                OnUserKicked?.Invoke(p_user);
            });

            _hubConnection.On<string, string, DateTime>("ReceiveMessage", (p_user, p_message, p_timeStamp) =>
            {
                OnMessageReceived?.Invoke(p_user, p_message, p_timeStamp);
            });

            _hubConnection.On<string, string, DateTime>("ReceiveImage", (p_user, p_base64Image, timeStamp) =>
            {
                OnImageReceived?.Invoke(p_user, p_base64Image, timeStamp);
            });

            try
            {
                if (_hubConnection != null && _hubConnection.State != HubConnectionState.Connected)
                {
                    await _hubConnection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Connection error: {ex.Message}", "OK");
            }
        }

        public Task AdminCreateRoomAsync(string p_roomName)
        {
            return _hubConnection?.InvokeAsync("AdminCreateRoom", p_roomName) ?? Task.CompletedTask;
        }

        public Task AdminDeleteRoomAsync(string p_roomName)
        {
            return _hubConnection?.InvokeAsync("AdminDeleteRoom", p_roomName) ?? Task.CompletedTask;
        }

        public Task KickUserAsync(string p_roomName, string p_userName)
        {
            return _hubConnection?.InvokeAsync("KickUserFromRoom", p_roomName, p_userName) ?? Task.CompletedTask;
        }

        public Task GetRoomListAsync()
        {
            return _hubConnection?.InvokeAsync("GetRoomList") ?? Task.CompletedTask;
        }

        public Task GetRoomUsersAsync(string p_roomName)
        {
            return _hubConnection?.InvokeAsync("GetRoomUsers", p_roomName) ?? Task.CompletedTask;
        }

        public Task AdminRefreshMessagesForRoom(string p_roomName)
        {
            return _hubConnection?.InvokeAsync("AdminRefreshMessagesForRoom", p_roomName) ?? Task.CompletedTask;
        }

        public Task LoadAllMessagesForRoom(string p_roomName)
        {
             return _hubConnection?.InvokeAsync("LoadAllMessagesForRoom", p_roomName) ?? Task.CompletedTask;
        }
    }
}
