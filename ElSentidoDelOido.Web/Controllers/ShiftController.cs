using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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

            var activos = tipos.Where(t => t.Enabled.GetValueOrDefault()).ToList();

            var model = new ShiftIndexViewModel
            {
                TiposTurnos = activos
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

            var result = horarios.Select(h => new
            {
                hora = h.Hour,
                disponible = h.Enabled,
                id = h.Id
            });

            return Json(result);
        }
    }
}
