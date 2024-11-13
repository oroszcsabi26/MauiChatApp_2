using CommunityToolkit.Maui;
using MauiChatApp.Interfaces;
using MauiChatApp.Services;
using MauiChatApp.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace MauiChatApp
{
    public static class MauiProgram
    {
        // Az alkalmazás belépési pontja, ez a metódus fut le először ez felelős az alkalmazás inicializálásáért, megadja hogy a fő alkalmazás az App osztály lesz
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder? builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Szolgáltatások regisztrálása a DI konténerben
            builder.Services.AddSingleton<IChatService, ChatService>();
            builder.Services.AddTransient<ChatViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            //metódus lehetővé teszi, hogy az alkalmazás életciklus eseményeit kezeljük, például amikor az alkalmazás ablakot hoz létre, vagy amikor az alkalmazás felfüggesztésre kerül. Ez különösen hasznos platform-specifikus beállítások esetén.
            builder.ConfigureLifecycleEvents(lifecycle =>
            {
#if WINDOWS     //Ez a metódus hozzáad egy Windows platform specifikus életciklus eseményt. Az AddWindows metódusban definiálható, hogyan kezeljük a Windows-specifikus eseményeket.
                lifecycle.AddWindows(windows =>
                    windows.OnWindowCreated((window) => //Ez az eseménykezelő akkor hívódik meg, amikor az alkalmazás ablakot hoz létre. Itt tudjuk beállítani az ablak méretét.
                    {
                        // Windows App SDK használata az ablakméret beállításához
                        nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window); //Ez a metódus visszaadja a natív Windows ablak handle-ját, amely egy egyedi azonosító az ablak számára a Windows operációs rendszerben.
                        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle); // Ez a metódus visszaadja az ablak ID-jét a natív Windows API használatával. Az AppWindow.GetFromWindowId metódus ezt az ID-t használja az AppWindow objektum eléréséhez.
                        AppWindow appWindow = AppWindow.GetFromWindowId(windowId); // Ez a metódus visszaadja az AppWindow objektumot az adott ablak ID alapján. Az AppWindow osztályt a Windows App SDK használja az ablak kezelésére, beleértve a méretezést.

                        appWindow.Resize(new SizeInt32(600, 670)); 
                    }));
#endif
            });

            return builder.Build();
        }
    }
}
