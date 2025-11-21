using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElSentidoDelOido.Web.Controllers
{
    public class ProfessionalController : Controller
    {
        private readonly IProfessionalService _service;

        public ProfessionalController(IProfessionalService service)
        {
            _service = service;
        }

        // GET: /Professional
        public async Task<IActionResult> Index()
        {
            var items = await _service.GetAllAsync();
            return View(items);
        }

        // GET: /Professional/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Professional/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalCreateDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Professional/Edit/5
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

        // POST: /Professional/Edit
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

        // POST: /Professional/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Professional/Reactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            await _service.ReactivateAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
