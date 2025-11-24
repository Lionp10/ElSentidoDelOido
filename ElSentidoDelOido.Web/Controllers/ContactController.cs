using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactMessageService _contactService;

        public ContactController(IContactMessageService contactService)
        {
            _contactService = contactService;
        }

        public async Task<IActionResult> Index()
        {
            var all = (await _contactService.GetAllAsync()) ?? Enumerable.Empty<ContactMessageDTO>();
            var model = all.Where(m => !m.Answered).OrderByDescending(m => m.CreatedAt).ToList();
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var msg = await _contactService.GetByIdAsync(id);
            if (msg == null) return NotFound();

            if (!msg.Read)
            {
                await _contactService.MarkAsReadAsync(id);
                msg = await _contactService.GetByIdAsync(id) ?? msg;
            }

            return View(msg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["Mensaje"] = "La respuesta no puede estar vacía.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _contactService.ReplyAsync(id, reply);
                TempData["Mensaje"] = "Respuesta registrada correctamente.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar la respuesta: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _contactService.MarkAsReadAsync(id);
                TempData["Mensaje"] = "Mensaje marcado como leído.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                TempData["Mensaje"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                await _contactService.MarkAllAsReadAsync();
                TempData["Mensaje"] = "Todos los mensajes marcados como leídos.";
            }
            catch (System.Exception ex)
            {
                TempData["Mensaje"] = $"Error al marcar todos como leídos: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contactService.DeleteAsync(id);
                TempData["Mensaje"] = "Mensaje eliminado correctamente.";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (System.Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
