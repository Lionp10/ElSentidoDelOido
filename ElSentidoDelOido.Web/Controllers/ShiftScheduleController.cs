using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class ShiftScheduleController : Controller
    {
        private readonly IShiftScheduleService _service;
        private readonly IShiftTypeService _shiftTypeService;
        private const int PageSize = 10;

        public ShiftScheduleController(IShiftScheduleService service, IShiftTypeService shiftTypeService)
        {
            _service = service;
            _shiftTypeService = shiftTypeService;
        }

        public async Task<IActionResult> Index(int? shiftTypeId, int page = 1)
        {
            var tipos = await _shiftTypeService.GetAllAsync();
            var (items, totalCount) = await _service.GetPagedAsync(shiftTypeId, page, PageSize);

            var model = new ShiftScheduleIndexViewModel
            {
                Items = items,
                ShiftTypes = tipos,
                CurrentPage = page,
                PageSize = PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize),
                SelectedShiftTypeId = shiftTypeId
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var tipos = await _shiftTypeService.GetAllAsync();
            ViewBag.ShiftTypes = new SelectList(tipos.Where(t => t.Enabled.GetValueOrDefault()), "Id", "Name");
            return View(new ShiftScheduleCreateDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShiftScheduleCreateDTO dto)
        {
            var tipos = await _shiftTypeService.GetAllAsync();
            ViewBag.ShiftTypes = new SelectList(tipos.Where(t => t.Enabled.GetValueOrDefault()), "Id", "Name", dto.ShiftTypeId);

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.CreateAsync(dto);
                TempData["Mensaje"] = "Horario creado correctamente.";
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
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();

            var dto = new ShiftScheduleUpdateDTO
            {
                Id = entity.Id,
                Hour = entity.Hour,
                ShiftTypeId = null 
            };

            var tipos = await _shiftTypeService.GetAllAsync();
            ViewBag.ShiftTypes = new SelectList(tipos.Where(t => t.Enabled.GetValueOrDefault()), "Id", "Name");

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShiftScheduleUpdateDTO dto)
        {
            var tipos = await _shiftTypeService.GetAllAsync();
            ViewBag.ShiftTypes = new SelectList(tipos.Where(t => t.Enabled.GetValueOrDefault()), "Id", "Name", dto.ShiftTypeId);

            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Mensaje"] = "Horario actualizado correctamente.";
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
                TempData["Mensaje"] = "Horario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
