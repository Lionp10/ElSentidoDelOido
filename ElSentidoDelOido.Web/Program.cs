using ElSentidoDelOido.Datos.DataContext;
using Microsoft.EntityFrameworkCore;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Datos.Repositories.Implementations;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Negocio.Services.Implementations;
using ElSentidoDelOido.Negocio.Helpers;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ElSentidoDelOidoDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios y servicios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IShiftTypeRepository, ShiftTypeRepository>();
builder.Services.AddScoped<IShiftTypeService, ShiftTypeService>();

builder.Services.AddScoped<IShiftScheduleRepository, ShiftScheduleRepository>();
builder.Services.AddScoped<IShiftScheduleService, ShiftScheduleService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Registrar MVC (Controllers + Views) y Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Rutas MVC: HomeController -> Index por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mantener Razor Pages si las usas
app.MapRazorPages();

app.Run();
