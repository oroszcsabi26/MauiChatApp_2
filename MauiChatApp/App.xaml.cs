namespace MauiChatApp
{
    public partial class App : Application
    {
        //A mainpage betöltése
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();  //MainPage = new MainPage();  // Nincs szükség AppShell-re effektíve mivel egyetlen oldalból áll az applikáció ami a MainPage
        }
    }
}
