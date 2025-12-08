using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class HolidaysController : Controller
    {
        private readonly IHolidaysService _service;

        public HolidaysController(IHolidaysService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _service.GetAllAsync();
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HolidaysCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.CreateAsync(dto);
                TempData["Mensaje"] = "Feriado creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var dto = new HolidaysUpdateDTO
            {
                Id = existing.Id,
                Date = existing.Date,
                Message = existing.Message
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HolidaysUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Mensaje"] = "Feriado actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            TempData["Mensaje"] = "Feriado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
