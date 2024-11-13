using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using MauiChatApp.Interfaces;
using MauiSharedContent.Models;
using MauiSharedContent.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using System;
using MauiChatApp.Services;

namespace MauiChatApp.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly IChatService m_chatService;
        private readonly IDispatcher m_dispatcher;
        private ICameraStreamService m_cameraStreamService;
        private string? m_selectedImageBase64;
        private ImageSource m_cameraPreviewImage;
        public event PropertyChangedEventHandler? PropertyChanged;
        private HashSet<string> BannedRooms = new HashSet<string>();
        private CameraView m_cameraView;
        private CancellationTokenSource? m_cancellationTokenSource;

        //-------------------- Constructor --------------------//
        public ChatViewModel(IChatService p_chatService, IDispatcher p_dispatcher, CameraView p_cameraView)
        {
            m_chatService = p_chatService;
            m_dispatcher = p_dispatcher;
            m_cameraView = p_cameraView;

            try
            {
                m_cameraStreamService = new CameraStreamService(m_cameraView);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera initialization failed: {ex.Message}");
                m_cameraStreamService = null;
            }

            if (m_cameraStreamService != null)
            {
                m_cameraStreamService.NewImageReceived += async (p_image) =>
                {
                    await OnNewImageReceived(p_image);
                };
            }

            m_chatService.OnMessageReceived += (p_user, p_message, p_timeStamp) =>
            {
                Messages.Add(new MessageModel { Text = $"{p_user}: {p_message}", IsOwnMessage = p_user == User.Name, TimeStamp = p_timeStamp });
            };

            m_chatService.OnImageReceived += (p_user, p_base64Image, p_timeStamp) =>
            {
                ImageSource? imageSource = ImageHelper.ConvertBase64ToImageSource(p_base64Image);
                Messages.Add(new MessageModel { Text = $"{p_user} sent an image:", ImageSource = imageSource, IsImage = true, IsOwnMessage = p_user == User.Name, TimeStamp = p_timeStamp});
            };

            m_chatService.OnVideoFrameReceived += (p_user, p_base64Image) =>
            {
                m_dispatcher.Dispatch(() =>
                {
                    CameraPreviewImage = ImageHelper.ConvertBase64ToImageSource(p_base64Image);
                });
            };

            m_chatService.OnUserKickedFromRoom += (p_room) =>
            {
                if (RoomName == p_room)
                {
                    IsChatting = false;
                    IsInRoom = false;
                    Messages.Clear();

                    if (!BannedRooms.Contains(p_room))
                    {
                        BannedRooms.Add(p_room);
                    }

                    m_dispatcher.Dispatch(() =>
                    {
                        Application.Current.MainPage.DisplayAlert("Kicked from Room", "You have been kicked from the room.", "OK");
                        UpdateCanJoinChat();
                        UpdateCanStartCamera();
                        OnPropertyChanged(nameof(IsChatting));
                        OnPropertyChanged(nameof(IsInRoom));
                    });
                }
            };

            m_chatService.OnRoomListReceived += (p_rooms) =>
            {
                m_dispatcher.Dispatch(() =>
                {
                    AvailableRooms.Clear();
                    foreach (string room in p_rooms)
                    {
                        AvailableRooms.Add(room);
                    }
                });
            };

            m_chatService.OnBannedFromRoom += (p_room) =>
            {
                IsChatting = false;
                IsInRoom = false;
                BannedRooms.Add(p_room);

                m_dispatcher.Dispatch(() =>
                {
                    Application.Current.MainPage.DisplayAlert("Banned", $"You are banned from the {p_room} room.", "OK");

                    // Make buttons inactive
                    UpdateCanJoinChat();
                    UpdateCanStartCamera();
                    OnPropertyChanged(nameof(CanSendImage));
                    OnPropertyChanged(nameof(IsChatting));
                    OnPropertyChanged(nameof(IsInRoom));
                });
            };

            m_chatService.OnRoomDeleted += (p_room) =>
            {
                if (RoomName == p_room)
                {
                    IsChatting = false;
                    IsInRoom = false;

                    m_dispatcher.Dispatch(() =>
                    {
                        Application.Current.MainPage.DisplayAlert("Room Deleted", $"The room '{p_room}' has been deleted.", "OK");
                        OnPropertyChanged(nameof(IsChatting));
                        OnPropertyChanged(nameof(IsInRoom));
                    });
                }
            };

            m_chatService.OnRoomUsersReceived += (p_users) =>
            {
                m_dispatcher.Dispatch(() =>
                {
                    // Add user if they arent in the list
                    foreach (var newUser in p_users)
                    {
                        if (!UsersInRoom.Any(user => user.Name == newUser.Name))
                        {
                            UsersInRoom.Add(newUser);
                        }
                    }

                    // remove users who are no longer in the list
                    for (int i = UsersInRoom.Count - 1; i >= 0; i--)
                    {
                        UserModel existingUser = UsersInRoom[i];
                        if (!p_users.Any(user => user.Name == existingUser.Name))
                        {
                            UsersInRoom.Remove(existingUser);
                        }
                    }

                    OnPropertyChanged(nameof(UsersInRoom));
                    IsInRoom = true;
                    OnPropertyChanged(nameof(IsInRoom));
                });
            };
        }

        //---------------- Observable Collections ---------------//
        public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
        public ObservableCollection<string> AvailableRooms { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<UserModel> UsersInRoom { get; set; } = new ObservableCollection<UserModel>();

        //-------------------- Propertyes ----------------------//
        private bool m_isRoomNameVisible = false;
        public bool IsRoomNameVisible
        {
            get => m_isRoomNameVisible;
            set
            {
                m_isRoomNameVisible = value;
                OnPropertyChanged(nameof(IsRoomNameVisible));
            }
        }

        private string m_newRoomname;
        public string NewRoomName
        {
            get => m_newRoomname;
            set
            {
                m_newRoomname = value;
                OnPropertyChanged(nameof(NewRoomName));
            }
        }

        private string m_selectedRoom;
        public string SelectedRoom
        {
            get => m_selectedRoom;
            set
            {
                if (m_selectedRoom != value)
                {
                    m_selectedRoom = value;
                    RoomName = m_selectedRoom;

                    if (BannedRooms.Contains(m_selectedRoom))
                    {
                        IsChatting = false;
                        IsInRoom = false;
                    }

                    UpdateCanJoinChat();
                    UpdateCanStartCamera();
                    OnPropertyChanged(nameof(SelectedRoom));

                    if (!string.IsNullOrEmpty(m_selectedRoom))
                    {
                        StartUserListRefreshTimer();
                    }
                    else
                    {
                        // stop timer if there is no selected room
                        m_cancellationTokenSource?.Cancel();
                    }
                }
            }
        }

        private bool m_isServerConnected = false;
        public bool IsServerConnected
        {
            get => m_isServerConnected;
            set
            {
                m_isServerConnected = value;
                OnPropertyChanged(nameof(IsServerConnected));
                UpdateCanJoinChat();
                UpdateCanStartCamera();
            }
        }

        private bool m_isInRoom = false;
        public bool IsInRoom
        {
            get => m_isInRoom;
            set
            {
                m_isInRoom = value;
                OnPropertyChanged(nameof(IsInRoom));
            }
        }

        private UserModel m_user = new UserModel();
        public UserModel User
        {
            get => m_user;
            set
            {
                m_user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        private string m_roomName;
        public string RoomName
        {
            get => m_roomName;
            set
            {
                m_roomName = value;
                OnPropertyChanged(nameof(RoomName));
                UpdateCanJoinChat();
            }
        }

        private string m_messageText;
        public string MessageText
        {
            get => m_messageText;
            set
            {
                m_messageText = value;
                OnPropertyChanged(nameof(MessageText));
            }
        }

        private bool m_isChatting;
        public bool IsChatting
        {
            get => m_isChatting;
            set
            {
                m_isChatting = value;
                OnPropertyChanged(nameof(IsChatting));
                UpdateCanStartCamera();
            }
        }

        private bool m_isCameraRunning = false;
        public bool IsCameraRunning
        {
            get => m_isCameraRunning;
            set
            {
                m_isCameraRunning = value;
                OnPropertyChanged(nameof(IsCameraRunning));
                OnPropertyChanged(nameof(IsCameraAvailable));
                UpdateCanStartCamera();
            }
        }

        public ImageSource CameraPreviewImage
        {
            get => m_cameraPreviewImage;
            set
            {
                m_cameraPreviewImage = value;
                OnPropertyChanged(nameof(CameraPreviewImage));
            }
        }

        public bool CanSendImage => !string.IsNullOrWhiteSpace(m_selectedImageBase64);
        public bool IsCameraAvailable => !IsCameraRunning;
        public bool CanStartCamera => IsCameraAvailable && IsServerConnected && IsChatting;
        public bool CanJoinChat => IsServerConnected && !string.IsNullOrWhiteSpace(SelectedRoom) && !IsChatting && !BannedRooms.Contains(SelectedRoom);

        //-------------------- Commands --------------------//
        public ICommand JoinServerCommand => new Command(async () => await JoinServer());
        public ICommand JoinChatCommand => new Command(async () => await JoinChat());
        public ICommand LeaveChatCommand => new Command(async () => await LeaveChat());
        public ICommand SendMessageCommand => new Command(async () => await SendMessage());
        public ICommand SelectImageCommand => new Command(async () => await SelectImage());
        public ICommand SendImageCommand => new Command(async () => await SendImage());
        public ICommand StartCameraCommand => new Command(async () => await StartCamera());
        public ICommand StopCameraCommand => new Command(() => StopCamera());
        public ICommand CreateRoomCommand => new Command(async () => await CreateRoom());
        public ICommand CancelNewRoomCommand => new Command(async () => await CancelNewRoom());
        public ICommand NewRoomCommand => new Command(() =>
        {
            IsRoomNameVisible = true;
        });

        //-------------------- Methods --------------------//
        private async Task JoinServer()
        {
            try
            {
                await m_chatService.ConnectAsync("http://192.168.1.111:5000/chatHub");
                IsServerConnected = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to join server: {ex.Message}", "OK");
            }
        }

        private async Task JoinChat()
        {
            if (string.IsNullOrWhiteSpace(User.Name))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Name cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedRoom))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please select a room.", "OK");
                return;
            }

            Messages.Clear();
            await m_chatService.JoinRoomAsync(User.Name, SelectedRoom);
            await m_chatService.LoadAllMessagesForRoom(SelectedRoom);

            IsChatting = true;
            IsInRoom = true;
            UpdateCanJoinChat();
            UpdateCanStartCamera();
            OnPropertyChanged(nameof(CanSendImage));
        }

        private async Task LeaveChat()
        {
            await m_chatService.LeaveRoomAsync(User.Name, RoomName);
            IsChatting = false;
            IsInRoom = false;

            Messages.Clear();

            // stop the timer when leaving the room
            m_cancellationTokenSource?.Cancel();

            UpdateCanJoinChat();
            UpdateCanStartCamera();
        }

        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                await m_chatService.SendMessageAsync(User.Name, RoomName, MessageText);
                MessageText = string.Empty;
            }
        }

        private async Task SelectImage()
        {
            FileResult? result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Please select an image"
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                using (MemoryStream? memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    m_selectedImageBase64 = Convert.ToBase64String(imageBytes);
                    OnPropertyChanged(nameof(CanSendImage));
                }
            }
        }

        private async Task SendImage()
        {
            if (!string.IsNullOrWhiteSpace(m_selectedImageBase64))
            {
                await m_chatService.SendImageAsync(User.Name, RoomName, m_selectedImageBase64);
                m_selectedImageBase64 = null;
                OnPropertyChanged(nameof(CanSendImage));
            }
        }

        private async Task StartCamera()
        {
            if (m_cameraStreamService == null)
            {
                await Application.Current.MainPage.DisplayAlert("Camera Not Available", "No camera is available on this device.", "OK");
                return;
            }

            try
            {
                if (await CheckAndRequestCameraPermission())
                {
                    if (!IsCameraRunning)
                    {
                        m_cameraView.IsEnabled = true;
                        m_cameraView.IsVisible = true;
                        IsCameraRunning = true;
                        await m_cameraStreamService.StartCameraStream();
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Camera Not Available", $"No camera is available on this device. {ex.Message}", "OK");
            }
        }

        private void StopCamera()
        {
            try
            {
                m_cameraStreamService.StopCameraStream();
                IsCameraRunning = false;
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", $"Failed to stop camera: {ex.Message}", "OK");
            }
        }

        private async Task<bool> CheckAndRequestCameraPermission()
        {
            try
            {
                PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to check/request camera permission: {ex.Message}", "OK");
                return false;
            }
        }

        private async Task CreateRoom()
        {
            if (string.IsNullOrWhiteSpace(NewRoomName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Room name cannot be empty.", "OK");
                return;
            }

            await m_chatService.CreateRoomAsync(NewRoomName);
            NewRoomName = string.Empty;
            IsRoomNameVisible = false;
        }

        private async Task CancelNewRoom()
        {
            NewRoomName = string.Empty;
            IsRoomNameVisible = false;
        }

        private async Task OnNewImageReceived(ImageSource p_image)
        {
            CameraPreviewImage = p_image;
            OnPropertyChanged(nameof(CameraPreviewImage));

            await SendCameraFrame(p_image);
        }

        private async Task SendCameraFrame(ImageSource p_image)
        {
            try
            {
                string base64Image = await ImageHelper.ConvertImageSourceToBase64(p_image);
                await m_chatService.SendVideoFrameAsync(User.Name, RoomName, base64Image);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to send camera frame: {ex.Message}", "OK");
            }
        }

        private async Task StartUserListRefreshTimer()
        {
            m_cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = m_cancellationTokenSource.Token;

            while (!token.IsCancellationRequested)
            {
                if (!string.IsNullOrWhiteSpace(SelectedRoom))
                {
                    await m_chatService.GetRoomUsersAsync(SelectedRoom);
                }

                await Task.Delay(1000, token); 
            }
        }

        private void UpdateCanStartCamera()
        {
            OnPropertyChanged(nameof(CanStartCamera));
        }

        private void UpdateCanJoinChat()
        {
            OnPropertyChanged(nameof(CanJoinChat));
        }

        protected void OnPropertyChanged(string p_propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p_propertyName));
        }
    }
}
