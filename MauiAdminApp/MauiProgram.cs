using MauiAdminApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MauiAdminApp.ViewModels;
using MauiAdminApp.Interfaces;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace MauiAdminApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            MauiAppBuilder? builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

                builder.Services.AddSingleton<IAdminChatService, AdminChatService>();
                builder.Services.AddTransient<AdminViewModel>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

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
