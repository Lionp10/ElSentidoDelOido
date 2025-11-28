using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Repositories.Implementations;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.Helpers;
using ElSentidoDelOido.Negocio.Services.Implementations;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

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

        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
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

builder.Services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

builder.Services.AddHttpClient<IGoogleRecaptchaService, GoogleRecaptchaService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddRazorPages();

var keysFolder = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys"));
if (!keysFolder.Exists) keysFolder.Create();

var dp = builder.Services.AddDataProtection()
    .SetApplicationName("ElSentidoDelOido")
    .PersistKeysToFileSystem(keysFolder);

var certThumbprint = builder.Configuration["DataProtection:CertificateThumbprint"];
if (!string.IsNullOrEmpty(certThumbprint))
{
    try
    {
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);
        var certs = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
        if (certs.Count > 0)
        {
            dp.ProtectKeysWithCertificate(certs[0]);
        }
        store.Close();
    }
    catch
    {
        // si falla, se intentará DPAPI abajo (o quedará sin encryptor)
    }
}
else if (OperatingSystem.IsWindows())
{
    try
    {
        dp.ProtectKeysWithDpapi(protectToLocalMachine: true);
    }
    catch
    {
        // si no es posible (hosting restringido), las claves quedarán sin cifrar
    }
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
