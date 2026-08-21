using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Hubs;
using VacanJefWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHostedService<MonitoreoSimuladoService>();

// Red de seguridad adicional: por defecto, si un servicio en segundo plano
// (como MonitoreoSimuladoService) deja escapar una excepción, ASP.NET Core
// apaga TODA la aplicación. Con "Ignore", un fallo ahí solo se registra en el
// log y el resto del sitio (login, reservas, facturas, etc.) sigue funcionando.
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddDbContext<VacanJefContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VacanJefDb")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "VacanJef.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Cuenta/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "camara",
    pattern: "camara/{token}",
    defaults: new { controller = "Camara", action = "Ver" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<MonitoreoHub>("/hubs/monitoreo");

app.Run();
