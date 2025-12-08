using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    [AdminOnly]
    public class ProfessionalController : Controller
    {
        private readonly IProfessionalService _service;

        public ProfessionalController(IProfessionalService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _service.GetAllAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalCreateDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();

            var editDto = new ProfessionalUpdateDTO
            {
                Id = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Email = item.Email,
                Phone = item.Phone,
                Address = item.Address,
                Enabled = item.Enabled.GetValueOrDefault()
            };

            return View(editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfessionalUpdateDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            await _service.ReactivateAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            var dto = new ProfessionalUpdateDTO
            {
                Id = item.Id,
                FirstName = item.FirstName,
                LastName = item.LastName,
                Email = item.Email,
                Phone = item.Phone,
                Address = item.Address,
                Enabled = true
            };

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Mensaje"] = "Profesional activado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al activar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
