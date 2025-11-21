using Microsoft.AspNetCore.Mvc;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ElSentidoDelOido.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Auth/Index
        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "User");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            var user = await _userService.AuthenticateAsync(dto.Email ?? string.Empty, dto.Password ?? string.Empty);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos");
                return View("Index");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, (user.RoleName ?? "User"))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = dto.RememberMe,
                ExpiresUtc = dto.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            return RedirectToAction("Index", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Auth");
        }
    }
}
