using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _role_service;

        public UserController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _role_service = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateRolesAsync();
            return View(new UserCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync();
                return View(dto);
            }

            try
            {
                var created = await _userService.CreateAsync(dto);

                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateRolesAsync();
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var dto = new UserUpdateDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId,
                Enabled = user.Enabled.GetValueOrDefault()
            };

            await PopulateRolesAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync();
                return View(dto);
            }

            try
            {
                var updated = await _userService.UpdateAsync(dto);

                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var curId) && curId == dto.Id)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, updated.Id.ToString()),
                        new Claim(ClaimTypes.Name, updated.Email ?? string.Empty),
                        new Claim("FullName", $"{updated.FirstName} {updated.LastName}"),
                        new Claim(ClaimTypes.Role, (updated.RoleName ?? "User"))
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var currentAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    var props = currentAuth?.Properties ?? new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
                }

                TempData["Mensaje"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateRolesAsync();
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var dto = new UserUpdateDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId,
                Enabled = false
            };

            try 
            {
                var updated = await _userService.UpdateAsync(dto);

                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out var curId) && curId == id)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["Mensaje"] = "Tu cuenta ha sido desactivada.";
                    return RedirectToAction("Index", "Auth");
                }

                TempData["Mensaje"] = "Usuario dado de baja correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al dar de baja: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var dto = new UserUpdateDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = user.RoleId,
                Enabled = true
            };

            try
            {
                var updated = await _userService.UpdateAsync(dto);

                TempData["Mensaje"] = "Usuario activado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al activar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PopulateRolesAsync()
        {
            var roles = await _role_service.GetActiveAsync();
            ViewBag.Roles = roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name ?? string.Empty })
                .ToList();
        }
    }
}
