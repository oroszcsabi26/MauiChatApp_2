using Microsoft.Maui.Controls;
using MauiAdminApp.Services;
using MauiAdminApp.ViewModels;

namespace MauiAdminApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            AdminViewModel adminViewModel = new AdminViewModel
                (new AdminChatService()
                );

            // A BindingContext beállítása a ViewModel-re
            BindingContext = adminViewModel;
        }
    }
}

