using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Data;
using MusicRadioInc.Interfaces;
using MusicRadioInc.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar Entity Framework Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar servicios de sesi�n
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n (ej. 30 minutos)
    options.Cookie.HttpOnly = true; // La cookie de sesi�n solo es accesible por el servidor
    options.Cookie.IsEssential = true; // Hace que la cookie de sesi�n sea esencial para el funcionamiento de la app
});

// --- Registrar tus servicios personalizados aqu� ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
// --- Fin del registro de servicios ---

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar servicios de sesi�n
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
