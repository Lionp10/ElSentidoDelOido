using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;

namespace ElSentidoDelOido.Web.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IShiftTypeService _shiftTypeService;
        private readonly IShiftScheduleService _shiftScheduleService;

        public ShiftController(IShiftTypeService shiftTypeService, IShiftScheduleService shiftScheduleService)
        {
            _shiftTypeService = shiftTypeService;
            _shiftScheduleService = shiftScheduleService;
        }

        public async Task<IActionResult> Index()
        {
            var tipos = await _shiftTypeService.GetAllAsync();
            var model = new ShiftIndexViewModel
            {
                TiposTurnos = tipos
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHorarios(int tipoTurnoId, string fecha)
        {
            if (!DateTime.TryParse(fecha, out var date))
            {
                return BadRequest("Fecha inválida");
            }

            var horarios = await _shiftScheduleService.GetAvailabilityAsync(tipoTurnoId, date.Date);

            // Retornar solo lo necesario para el JS
            var result = horarios.Select(h => new
            {
                hora = h.Hora,
                disponible = h.Disponible,
                id = h.Id
            });

            return Json(result);
        }
    }
}
