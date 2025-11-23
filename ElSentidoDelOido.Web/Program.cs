using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Repositories.Implementations;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.Helpers;
using ElSentidoDelOido.Negocio.Services.Implementations;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ElSentidoDelOidoDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Index";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "EsdoAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// Repositorios y servicios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IShiftTypeRepository, ShiftTypeRepository>();
builder.Services.AddScoped<IShiftTypeService, ShiftTypeService>();

builder.Services.AddScoped<IShiftScheduleRepository, ShiftScheduleRepository>();
builder.Services.AddScoped<IShiftScheduleService, ShiftScheduleService>();

builder.Services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
builder.Services.AddScoped<IProfessionalService, ProfessionalService>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddScoped<IHolidaysRepository, HolidaysRepository>();
builder.Services.AddScoped<IHolidaysService, HolidaysService>();

builder.Services.AddScoped<IShiftRepository, ShiftRepository>();
builder.Services.AddScoped<IShiftService, ShiftService>();

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
