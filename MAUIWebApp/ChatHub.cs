using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private static List<string> Rooms = new List<string>();
    private static Dictionary<string, List<string>> RoomUsers = new Dictionary<string, List<string>>();

    // Store messages with timestamp and type
    private static Dictionary<string, List<(string Message, DateTime Timestamp, bool IsImage)>> RoomMessages = new Dictionary<string, List<(string, DateTime, bool)>>();

    // Track last query time for each room
    private static Dictionary<string, DateTime> LastFetchedMessageTime = new Dictionary<string, DateTime>();

    private static Dictionary<string, string> UserConnectionIds = new Dictionary<string, string>();
    private static Dictionary<string, List<string>> BannedUsers = new Dictionary<string, List<string>>();
    private DateTime m_lastLoadedMessageTimeStamp;
    private DateTime timeStamp;

    public async Task CreateRoom(string p_roomName)
    {
        if (!Rooms.Contains(p_roomName))
        {
            Rooms.Add(p_roomName);
            RoomUsers[p_roomName] = new List<string>();
            RoomMessages[p_roomName] = new List<(string, DateTime, bool)>();
            await Clients.All.SendAsync("ReceiveRoomList", Rooms);
        }
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveRoomList", Rooms);
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(string p_user, string p_room, string p_message)
    {
        if (RoomUsers.ContainsKey(p_room) && RoomUsers[p_room].Contains(p_user))
        {
            timeStamp = DateTime.Now;
            RoomMessages[p_room].Add(($"{p_user}: {p_message}", timeStamp, false));

            await Clients.Group(p_room).SendAsync("ReceiveMessage", p_user, p_message, timeStamp);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "You are not a member of this room.");
        }
    }

    public async Task JoinRoom(string p_user, string p_room)
    {
        if (BannedUsers.ContainsKey(p_room) && BannedUsers[p_room].Contains(p_user))
        {
            await Clients.Caller.SendAsync("BannedFromRoom", p_room);
            return;
        }

        timeStamp = DateTime.Now;

        if (RoomUsers.ContainsKey(p_room))
        {
            RoomUsers[p_room].Add(p_user);

            // Save the user's connection ID
            UserConnectionIds[p_user] = Context.ConnectionId;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, p_room);
        await Clients.Group(p_room).SendAsync("ReceiveMessage", "System", $"{p_user} has joined the room.", timeStamp);
    }

    private string GetConnectionIdForUser(string p_userName)
    {
        return UserConnectionIds.ContainsKey(p_userName) ? UserConnectionIds[p_userName] : null;
    }

    public async Task LeaveRoom(string p_user, string p_room)
    {
        if (RoomUsers.ContainsKey(p_room))
        {
            RoomUsers[p_room].Remove(p_user);
        }
        
        timeStamp = DateTime.Now;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, p_room);
        await Clients.Group(p_room).SendAsync("ReceiveMessage", "System", $"{p_user} leaved the room.", timeStamp);
    }

    public async Task SendImage(string p_user, string p_room, string p_base64Image)
    {
        if (RoomUsers.ContainsKey(p_room) && RoomUsers[p_room].Contains(p_user))
        {
            DateTime timestamp = DateTime.Now;
            RoomMessages[p_room].Add(($"{p_user}: {p_base64Image}", timestamp, true));

            await Clients.Group(p_room).SendAsync("ReceiveImage", p_user, p_base64Image, timestamp);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "You are not a member of this room.");
        }
    }

    public async Task SendVideoFrame(string p_user, string p_room, string p_base64Image)
    {
        try
        {
            await Clients.Group(p_room).SendAsync("ReceiveVideoFrame", p_user, p_base64Image);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending video frame: {ex.Message}");
            throw;
        }
    }

    public async Task LoadAllMessagesForRoom(string p_roomName)
    {
        if (RoomMessages.ContainsKey(p_roomName))
        {
            foreach (var (message, timestamp, isImage) in RoomMessages[p_roomName])
            {
                string[] parts = message.Split(": ", 2);
                if (parts.Length == 2)
                {
                    string user = parts[0];
                    string content = parts[1];

                    if (isImage)
                    {
                        await Clients.Caller.SendAsync("ReceiveImage", user, content, timestamp);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", user, content, timestamp);
                    }
                }
                m_lastLoadedMessageTimeStamp = timestamp;
                LastFetchedMessageTime[p_roomName] = m_lastLoadedMessageTimeStamp;
            }
        }
    }

    public async Task AdminRefreshMessagesForRoom(string p_roomName)
    {
        DateTime lastFetchedTime = LastFetchedMessageTime.ContainsKey(p_roomName) ? LastFetchedMessageTime[p_roomName] : DateTime.MinValue;
        LastFetchedMessageTime[p_roomName] = DateTime.Now;

        if (RoomMessages.ContainsKey(p_roomName))
        {
            var newMessages = RoomMessages[p_roomName]
                .Where(msg => msg.Timestamp > lastFetchedTime)
                .ToList();

            foreach (var (message, timestamp, isImage) in newMessages)
            {
                string[] parts = message.Split(": ", 2);
                if (parts.Length == 2)
                {
                    string user = parts[0];
                    string content = parts[1];

                    if (isImage)
                    {
                        await Clients.Caller.SendAsync("ReceiveImage", user, content, timestamp);
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("ReceiveMessage", user, content, timestamp);
                    }
                }
            }
        }
    }

    public async Task GetRoomUsers(string p_roomName)
    {
        if (RoomUsers.ContainsKey(p_roomName))
        {
            await Clients.Caller.SendAsync("ReceiveRoomUsers", RoomUsers[p_roomName]);
        }
    }

    //---------------------Admin functions------------------//

    public async Task AdminCreateRoom(string p_roomName)
    {
        if (!Rooms.Contains(p_roomName))
        {
            Rooms.Add(p_roomName);
            RoomUsers[p_roomName] = new List<string>();
            RoomMessages[p_roomName] = new List<(string, DateTime, bool)>();
            await Clients.All.SendAsync("ReceiveRoomList", Rooms);
        }
    }

    public async Task GetRoomList()
    {
        await Clients.Caller.SendAsync("ReceiveRoomList", Rooms);
    }

    public async Task AdminDeleteRoom(string p_roomName)
    {
        if (Rooms.Contains(p_roomName))
        {
            await Clients.Group(p_roomName).SendAsync("RoomDeleted", p_roomName);

            if (RoomUsers.ContainsKey(p_roomName))
            {
                foreach (string user in RoomUsers[p_roomName])
                {
                    string connectionId = Context.ConnectionId;
                    await Groups.RemoveFromGroupAsync(connectionId, p_roomName);
                }
            }

            Rooms.Remove(p_roomName);
            RoomUsers.Remove(p_roomName);
            RoomMessages.Remove(p_roomName);
            await Clients.All.SendAsync("ReceiveRoomList", Rooms);
        }
    }

    public async Task KickUserFromRoom(string p_roomName, string p_userName)
    {
        if (RoomUsers.ContainsKey(p_roomName) && RoomUsers[p_roomName].Contains(p_userName))
        {
            RoomUsers[p_roomName].Remove(p_userName);

            // Search for the user's connection ID
            string userConnectionId = GetConnectionIdForUser(p_userName);
            if (userConnectionId != null)
            {
                timeStamp = DateTime.Now;
                // Delete the user from the group
                await Clients.Group(p_roomName).SendAsync("ReceiveMessage", "System", $"{p_userName} has been kicked out of the room.", timeStamp);
                await Groups.RemoveFromGroupAsync(userConnectionId, p_roomName);

                // Send a message to the user about the kick
                await Clients.Client(userConnectionId).SendAsync("UserKickedFromRoom", p_roomName);
            }

            // Add user to the banned list
            if (!BannedUsers.ContainsKey(p_roomName))
            {
                BannedUsers[p_roomName] = new List<string>();
            }
            BannedUsers[p_roomName].Add(p_userName);
        }
    }
}

