using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class ShiftTypeController : Controller
    {
        private readonly IShiftTypeService _service;

        public ShiftTypeController(IShiftTypeService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _service.GetAllAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ShiftTypeCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShiftTypeCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.CreateAsync(dto);
                TempData["Mensaje"] = "Tipo de turno creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            var dto = new ShiftTypeUpdateDTO
            {
                Id = item.Id,
                Name = item.Name,
                Enabled = item.Enabled.GetValueOrDefault()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShiftTypeUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Mensaje"] = "Tipo de turno actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                TempData["Mensaje"] = "Tipo de turno desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al desactivar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                await _service.ReactivateAsync(id);
                TempData["Mensaje"] = "Tipo de turno reactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al reactivar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
