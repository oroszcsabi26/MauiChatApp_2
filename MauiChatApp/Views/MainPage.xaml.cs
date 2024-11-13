using MauiChatApp.ViewModels;
using MauiChatApp.Services;
using MauiChatApp.Interfaces;

namespace MauiChatApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            ChatViewModel viewModel = new ChatViewModel(
                new ChatService(),
                Dispatcher,
                MyCamera
            );

            BindingContext = viewModel;
        }
    }
}




