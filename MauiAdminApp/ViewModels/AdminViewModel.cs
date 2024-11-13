using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiAdminApp.Services;
using MauiAdminApp.Interfaces;
using MauiSharedContent.Helpers;
using MauiSharedContent.Models;

namespace MauiAdminApp.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly IAdminChatService m_adminChatService;
        private CancellationTokenSource? m_cancellationTokenSource;
        public event PropertyChangedEventHandler? PropertyChanged;

        public AdminViewModel(IAdminChatService p_adminChatService)
        {
            m_adminChatService = p_adminChatService;

            m_adminChatService.OnRoomListReceived += (p_rooms) =>
            {
                Rooms = new ObservableCollection<string>(p_rooms);
                OnPropertyChanged(nameof(Rooms));
            };

            m_adminChatService.OnRoomUsersReceived += (p_users) =>
            {
                UpdateUserList(p_users);
            };

            m_adminChatService.OnMessageReceived += (p_user, p_message, p_timeStamp) =>
            {
                Messages.Add(new MessageModel { Text = $"{p_user}: {p_message}", TimeStamp = p_timeStamp, IsImage = false });
                OnPropertyChanged(nameof(Messages));
            };

            m_adminChatService.OnImageReceived += (p_user, p_base64Image, p_timestamp) =>
            {
                ImageSource? imageSource = ImageHelper.ConvertBase64ToImageSource(p_base64Image);
                Messages.Add(new MessageModel { Text = $"{p_user} sent an image:", ImageSource = imageSource, TimeStamp = p_timestamp, IsImage = true });
                OnPropertyChanged(nameof(Messages));
            };
        }

        //--------------------Properties--------------------------//
        private string m_newRoomName = string.Empty;
        public string NewRoomName
        {
            get => m_newRoomName;
            set
            {
                m_newRoomName = value;
                OnPropertyChanged(nameof(NewRoomName));
            }
        }

        private string m_selectedRoom = string.Empty;
        public string SelectedRoom
        {
            get => m_selectedRoom;
            set
            {
                if (m_selectedRoom != value)
                {
                    m_selectedRoom = value;
                    OnPropertyChanged(nameof(SelectedRoom));

                    if (!string.IsNullOrWhiteSpace(m_selectedRoom))
                    {
                        Messages.Clear();
                        m_adminChatService.LoadAllMessagesForRoom(m_selectedRoom);
                        StartRoomDataRefreshTimer();
                    }
                    else
                    {
                        m_cancellationTokenSource?.Cancel();
                    }
                }
            }
        }

        private string m_selectedUser = string.Empty;
        public string SelectedUser
        {
            get => m_selectedUser;
            set
            {
                m_selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        //-----------------------Collections----------------------//
        public ObservableCollection<string> Rooms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> UsersInRoom { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

        //-----------------------Commands-------------------------//
        public ICommand CreateRoomCommand => new Command(async () => await CreateRoom());
        public ICommand DeleteRoomCommand => new Command(async () => await DeleteRoom());
        public ICommand KickUserCommand => new Command(async () => await KickUser());
        public ICommand JoinServerCommand => new Command(async () => await JoinServer());

        //---------------Command implementations------------------//
        private async Task JoinServer()
        {
            try
            {
                await m_adminChatService.ConnectAsync("http://192.168.1.111:5000/chatHub");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to join server: {ex.Message}", "OK");
            }

            await m_adminChatService.GetRoomListAsync();
        }

        private async Task CreateRoom()
        {
            if (!string.IsNullOrWhiteSpace(NewRoomName))
            {
                await m_adminChatService.AdminCreateRoomAsync(NewRoomName);
                NewRoomName = string.Empty;
            }
        }

        private async Task DeleteRoom()
        {
            if (!string.IsNullOrWhiteSpace(SelectedRoom))
            {
                await m_adminChatService.AdminDeleteRoomAsync(SelectedRoom);
            }
        }

        private async Task KickUser()
        {
            if (!string.IsNullOrWhiteSpace(SelectedRoom) && !string.IsNullOrWhiteSpace(SelectedUser))
            {
                await m_adminChatService.KickUserAsync(SelectedRoom, SelectedUser);
            }
        }

        private void UpdateUserList(List<string> p_updatedUsers)
        {
            foreach (string newUser in p_updatedUsers)
            {
                if (!UsersInRoom.Contains(newUser))
                {
                    UsersInRoom.Add(newUser);
                }
            }

            for (int i = UsersInRoom.Count - 1; i >= 0; i--)
            {
                if (!p_updatedUsers.Contains(UsersInRoom[i]))
                {
                    UsersInRoom.Remove(UsersInRoom[i]);
                }
            }

            OnPropertyChanged(nameof(UsersInRoom));
        }

        private async Task StartRoomDataRefreshTimer()
        {
            m_cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = m_cancellationTokenSource.Token;

            while (!token.IsCancellationRequested)
            {
                if (!string.IsNullOrWhiteSpace(SelectedRoom))
                {
                    await m_adminChatService.GetRoomUsersAsync(SelectedRoom);
                    await m_adminChatService.AdminRefreshMessagesForRoom(SelectedRoom);
                }
                await Task.Delay(1000, token);
            }
        }

        protected void OnPropertyChanged(string p_propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p_propertyName));
        }
    }
}
