using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using System;

namespace ElSentidoDelOido.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Index
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserCreateDTO());
        }

        // POST: Create (recibe DTO directamente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                // _userService.CreateAsync ya acepta UserCreateDTO
                var created = await _userService.CreateAsync(dto);

                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }
    }
}
