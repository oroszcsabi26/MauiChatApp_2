//using MAUIWebApp;
//Ez a f�jl tartalmazza az ASP.NET Core alkalmaz�s konfigur�ci�j�t, bele�rtve a SignalR be�ll�t�sokat is.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args); //L�trehozza az alkalmaz�s konfigur�ci�s objektumot, amely lehet�v� teszi az alkalmaz�s be�ll�t�sainak testreszab�s�t.

        // Szolg�ltat�sok hozz�ad�sa a kont�nerhez.
        builder.Services.AddControllers(); //Ez a k�d hozz�adja a kontroller szolg�ltat�st az alkalmaz�shoz. A kontroller szolg�ltat�s lehet�v� teszi az API v�gpontok kezel�s�t.                                  //builder.Services.AddSignalR(); //Ez a k�d hozz�adja a SignalR szolg�ltat�st az alkalmaz�shoz. A SignalR szolg�ltat�s lehet�v� teszi a val�s idej� kommunik�ci�t a kliensek �s a szerverek k�z�tt.
        builder.Services.AddEndpointsApiExplorer(); //Ez a k�d hozz�adja az API felfedez� szolg�ltat�st az alkalmaz�shoz. Az API felfedez� szolg�ltat�s lehet�v� teszi az API v�gpontok felfedez�s�t �s dokument�l�s�t.
        builder.Services.AddSwaggerGen(); //Ez a k�d hozz�adja a SwaggerGen szolg�ltat�st az alkalmaz�shoz. A SwaggerGen szolg�ltat�s lehet�v� teszi az API dokument�l�s�t �s a Swagger felhaszn�l�i fel�let l�trehoz�s�t.
        builder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
        });
        // CORS be�ll�t�sok hozz�ad�sa az alkalmaz�shoz, A CORS lehet�v� teszi, hogy a k�l�nb�z� domain-ekr�l �rkez� k�r�sek kezelhet�ek legyenek.
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("http://192.168.1.111:5000") // Minden forr�st enged�lyez�nk. eredeti k�dban : builder.WithOrigins(" // enged�lyezi a k�r�seket a megadott eredetr�l (a fejleszt�i g�p IP c�m�r�l �s portj�r�l).
                       .AllowAnyHeader() // enged�lyezi az �sszes fejl�cet
                       .AllowAnyMethod() // enged�lyezi az �sszes HTTP m�dszert
                       .AllowCredentials(); // enged�lyezi a hiteles�t�st (ezt csak akkor hagyd meg, ha t�nyleg sz�ks�ges)
            });
        });

        // HTTP �s HTTPS figyel�s be�ll�t�sa
        builder.WebHost.ConfigureKestrel(options => //konfigur�lja a Kestrel webszervert, amely az ASP.NET Core alap�rtelmezett be�p�tett webszervere.
        {
            options.ListenAnyIP(5000); //Ez a sor be�ll�tja a Kestrel szervert, hogy hallgasson az 5000-es porton HTTP protokollon kereszt�l, b�rmilyen IP c�mr�l �rkez� k�r�sekre.
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.UseHttps(); // HTTPS
            });
        });

        WebApplication app = builder.Build(); // l�trehoz egy WebApplication objektumot, amely k�pviseli az ASP.NET Core alkalmaz�st. A Build met�dus �ssze�ll�tja az alkalmaz�st a kor�bban be�ll�tott konfigur�ci�k alapj�n.Az alkalmaz�s konfigur�ci�j�nak befejez�se �s az alkalmaz�s objektum l�trehoz�sa.

        // HTTP k�r�s feldolgoz�si cs�vezet�k konfigur�l�sa.
        if (app.Environment.IsDevelopment()) // Ez a felt�tel ellen�rzi, hogy az alkalmaz�s fejleszt�si k�rnyezetben fut-e.
        {
            app.UseDeveloperExceptionPage(); // Ez a k�d hozz�adja a fejleszt�i hibakezel� oldalt az alkalmaz�shoz. A fejleszt�i hibakezel� oldal seg�t a hib�k gyorsabb azonos�t�s�ban �s jav�t�s�ban a fejleszt�s sor�n.
            app.UseSwagger(); // Ez a k�d hozz�adja a Swagger middleware-t az alkalmaz�shoz. A Swagger middleware lehet�v� teszi az API dokument�l�s�t �s a Swagger felhaszn�l�i fel�let l�trehoz�s�t.
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1")); // Ez a k�d hozz�adja a Swagger felhaszn�l�i fel�letet az alkalmaz�shoz. A Swagger felhaszn�l�i fel�let lehet�v� teszi az API v�gpontok felfedez�s�t �s tesztel�s�t.
        }

        app.UseRouting(); // Ez a k�d hozz�adja a routing middleware-t az alkalmaz�shoz. A routing middleware lehet�v� teszi az HTTP k�r�sek �tvonalakra ir�ny�t�s�t �s a megfelel� kontrollerekhez val� tov�bb�t�s�t.

        // CORS middleware hozz�ad�sa
        app.UseCors(); // enged�lyezi a CORS middleware haszn�lat�t, amely alkalmazza a kor�bban defini�lt CORS szab�lyokat.

        // Ez a k�dblokk konfigur�lja az alkalmaz�s v�gpontjait.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // Be�ll�tja a kontroller v�gpontokat az alkalmaz�shoz. A kontroller v�gpontok lehet�v� teszik az API v�gpontok kezel�s�t. Be�ll�tja az �sszes MVC Controller alap� v�gpontot.
            endpoints.MapHub<ChatHub>("/chatHub"); // Be�ll�tja a SignalR hub v�gpontot. a /chatHub URL-en, amely a ChatHub oszt�lyt haszn�lja.
        });

        app.Run(); // Elind�tja az alkalmaz�st �s elkezdi figyelni a bej�v� k�r�seket.
    }
}