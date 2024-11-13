//using MAUIWebApp;
//Ez a fájl tartalmazza az ASP.NET Core alkalmazás konfigurációját, beleértve a SignalR beállításokat is.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args); //Létrehozza az alkalmazás konfigurációs objektumot, amely lehetõvé teszi az alkalmazás beállításainak testreszabását.

        // Szolgáltatások hozzáadása a konténerhez.
        builder.Services.AddControllers(); //Ez a kód hozzáadja a kontroller szolgáltatást az alkalmazáshoz. A kontroller szolgáltatás lehetõvé teszi az API végpontok kezelését.                                  //builder.Services.AddSignalR(); //Ez a kód hozzáadja a SignalR szolgáltatást az alkalmazáshoz. A SignalR szolgáltatás lehetõvé teszi a valós idejû kommunikációt a kliensek és a szerverek között.
        builder.Services.AddEndpointsApiExplorer(); //Ez a kód hozzáadja az API felfedezõ szolgáltatást az alkalmazáshoz. Az API felfedezõ szolgáltatás lehetõvé teszi az API végpontok felfedezését és dokumentálását.
        builder.Services.AddSwaggerGen(); //Ez a kód hozzáadja a SwaggerGen szolgáltatást az alkalmazáshoz. A SwaggerGen szolgáltatás lehetõvé teszi az API dokumentálását és a Swagger felhasználói felület létrehozását.
        builder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
        });
        // CORS beállítások hozzáadása az alkalmazáshoz, A CORS lehetõvé teszi, hogy a különbözõ domain-ekrõl érkezõ kérések kezelhetõek legyenek.
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://192.168.1.111:5000") // Minden forrást engedélyezünk. eredeti kódban : builder.WithOrigins(" // engedélyezi a kéréseket a megadott eredetrõl (a fejlesztõi gép IP címérõl és portjáról).
                       .AllowAnyHeader() // engedélyezi az összes fejlécet
                       .AllowAnyMethod() // engedélyezi az összes HTTP módszert
                       .AllowCredentials(); // engedélyezi a hitelesítést (ezt csak akkor hagyd meg, ha tényleg szükséges)
            });
        });

        // HTTP és HTTPS figyelés beállítása
        builder.WebHost.ConfigureKestrel(options => //konfigurálja a Kestrel webszervert, amely az ASP.NET Core alapértelmezett beépített webszervere.
        {
            options.ListenAnyIP(5000); //Ez a sor beállítja a Kestrel szervert, hogy hallgasson az 5000-es porton HTTP protokollon keresztül, bármilyen IP címrõl érkezõ kérésekre.
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.UseHttps(); // HTTPS
            });
        });

        WebApplication app = builder.Build(); // létrehoz egy WebApplication objektumot, amely képviseli az ASP.NET Core alkalmazást. A Build metódus összeállítja az alkalmazást a korábban beállított konfigurációk alapján.Az alkalmazás konfigurációjának befejezése és az alkalmazás objektum létrehozása.

        // HTTP kérés feldolgozási csõvezeték konfigurálása.
        if (app.Environment.IsDevelopment()) // Ez a feltétel ellenõrzi, hogy az alkalmazás fejlesztési környezetben fut-e.
        {
            app.UseDeveloperExceptionPage(); // Ez a kód hozzáadja a fejlesztõi hibakezelõ oldalt az alkalmazáshoz. A fejlesztõi hibakezelõ oldal segít a hibák gyorsabb azonosításában és javításában a fejlesztés során.
            app.UseSwagger(); // Ez a kód hozzáadja a Swagger middleware-t az alkalmazáshoz. A Swagger middleware lehetõvé teszi az API dokumentálását és a Swagger felhasználói felület létrehozását.
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1")); // Ez a kód hozzáadja a Swagger felhasználói felületet az alkalmazáshoz. A Swagger felhasználói felület lehetõvé teszi az API végpontok felfedezését és tesztelését.
        }

        app.UseRouting(); // Ez a kód hozzáadja a routing middleware-t az alkalmazáshoz. A routing middleware lehetõvé teszi az HTTP kérések útvonalakra irányítását és a megfelelõ kontrollerekhez való továbbítását.

        // CORS middleware hozzáadása
        app.UseCors(); // engedélyezi a CORS middleware használatát, amely alkalmazza a korábban definiált CORS szabályokat.

        // Ez a kódblokk konfigurálja az alkalmazás végpontjait.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // Beállítja a kontroller végpontokat az alkalmazáshoz. A kontroller végpontok lehetõvé teszik az API végpontok kezelését. Beállítja az összes MVC Controller alapú végpontot.
            endpoints.MapHub<ChatHub>("/chatHub"); // Beállítja a SignalR hub végpontot. a /chatHub URL-en, amely a ChatHub osztályt használja.
        });

        app.Run(); // Elindítja az alkalmazást és elkezdi figyelni a bejövõ kéréseket.
    }
}