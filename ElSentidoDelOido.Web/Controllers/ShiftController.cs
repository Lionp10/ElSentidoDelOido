using ElSentidoDelOido.Datos.Entities.Enums;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;

namespace ElSentidoDelOido.Web.Controllers
{
    public class ShiftController : Controller
    {
        #region Dependency Injection
        
        private readonly IShiftTypeService _shiftTypeService;
        private readonly IShiftScheduleService _shiftScheduleService;
        private readonly IHolidaysService _holidaysService;
        private readonly IShiftService _shiftService;
        private readonly IProfessionalService _professional_service;

        public ShiftController(
            IShiftTypeService shiftTypeService, 
            IShiftScheduleService shiftScheduleService, 
            IHolidaysService holidaysService,
            IShiftService shiftService,
            IProfessionalService professionalService)
        {
            _shiftTypeService = shiftTypeService;
            _shiftScheduleService = shiftScheduleService;
            _holidaysService = holidaysService;
            _shiftService = shiftService;
            _professional_service = professionalService;
        }

        #endregion

        #region Público - Solicitud de Turnos

        /// <summary>
        /// Vista pública para solicitar turnos
        /// </summary>
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

        /// <summary>
        /// Procesa la solicitud de turno del paciente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTurn(ShiftCreateDTO dto)
        {            
            if (!ModelState.IsValid)
            {
                var tipos = await _shiftTypeService.GetAllAsync();
                var activos = tipos.Where(t => t.Enabled.GetValueOrDefault()).ToList();
                
                ViewBag.Errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                var model = new ShiftIndexViewModel
                {
                    TiposTurnos = activos
                };
                return View("Index", model);
            }

            try
            {
                var turnoCreado = await _shiftService.CreateAsync(dto);
                
                TempData["MensajeExito"] = "¡Turno solicitado correctamente! Te contactaremos pronto para confirmar.";
                TempData["TurnoId"] = turnoCreado.Id;

                return RedirectToAction(nameof(Confirmation));
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado. Por favor, intenta nuevamente.");
            }

            var tiposError = await _shiftTypeService.GetAllAsync();
            var activosError = tiposError.Where(t => t.Enabled.GetValueOrDefault()).ToList();
            
            var modelError = new ShiftIndexViewModel
            {
                TiposTurnos = activosError
            };
            return View("Index", modelError);
        }

        /// <summary>
        /// Página de confirmación después de solicitar un turno
        /// </summary>
        public IActionResult Confirmation()
        {            
            if (TempData["MensajeExito"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Mensaje = TempData["MensajeExito"];
            ViewBag.TurnoId = TempData["TurnoId"];
            
            return View();
        }

        #endregion

        #region API Pública - Consulta de Horarios

        /// <summary>
        /// API para obtener horarios disponibles por tipo de turno y fecha
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerHorarios(int tipoTurnoId, string fecha)
        {
            if (!DateTime.TryParse(fecha, out var date))
            {
                return BadRequest("Fecha inválida");
            }

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Json(new { esFinDeSemana = true, mensaje = "Los consultorios permanecen cerrados los fines de semana." });
            }

            var feriados = await _holidaysService.GetAllAsync();
            var esFeriado = feriados.Any(f => f.Date.Date == date.Date);
            
            if (esFeriado)
            {
                var feriado = feriados.First(f => f.Date.Date == date.Date);
                var mensajeFeriado = "No hay horarios disponibles para este día.";

                return Json(new { esFeriado = true, mensaje = mensajeFeriado });
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

        /// <summary>
        /// API para obtener feriados del mes
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerFeriadosDelMes(int year, int month)
        {
            var feriados = await _holidaysService.GetAllAsync();
            var feriadosDelMes = feriados
                .Where(f => f.Date.Year == year && f.Date.Month == month)
                .Select(f => f.Date.Day)
                .ToList();

            return Json(feriadosDelMes);
        }

        #endregion

        #region Panel Administrativo - Gestión de Turnos

        /// <summary>
        /// Vista principal del panel administrativo para gestionar turnos
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Main(
            DateTime? fecha, 
            string? estado, 
            int? tipoTurnoId, 
            int? professionalId, 
            int page = 1, 
            int pageSize = 10)
        {
            var shiftTypes = await _shiftTypeService.GetAllAsync();
            var professionals = await _professional_service.GetAllAsync();

            var estadosDisponibles = new Dictionary<string, string>
            {
                { ShiftStateEnum.Pendiente.ToString(), "Pendiente" },
                { ShiftStateEnum.Confirmado.ToString(), "Confirmado" },
                { ShiftStateEnum.Rechazado.ToString(), "Rechazado" },
                { ShiftStateEnum.Cancelado.ToString(), "Cancelado" },
                { ShiftStateEnum.Culminado.ToString(), "Culminado" }
            };

            // Obtener turnos paginados con filtros
            var (items, totalCount) = await _shiftService.GetPagedAsync(fecha, estado, tipoTurnoId, professionalId, page, pageSize);

            var model = new ShiftMainViewModel
            {
                Items = items,
                FechaFiltro = fecha,
                EstadoFiltro = estado,
                TipoTurnoFiltro = tipoTurnoId,
                ProfessionalFiltro = professionalId,
                ShiftTypes = shiftTypes,
                Professionals = professionals.Where(p => p.Enabled.GetValueOrDefault()), // Solo activos
                EstadosDisponibles = estadosDisponibles,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(model);
        }

        /// <summary>
        /// GET: Editar turno — solo para turnos cuyo estado != Culminado y cuya fecha+hora ya pasó
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();

            // Calcular fecha/hora completa del turno si está disponible
            DateTime? turnoDateTime = null;
            if (shift.Date.HasValue && !string.IsNullOrEmpty(shift.ScheduleHour)
                && TimeSpan.TryParse(shift.ScheduleHour, out var shiftTime))
            {
                turnoDateTime = shift.Date.Value.Date.Add(shiftTime);
            }

            // Denegar edición si el turno está "Culminado" O si la fecha+hora ya pasó.
            if (string.Equals(shift.ShiftStateId, ShiftStateEnum.Culminado.ToString(), StringComparison.OrdinalIgnoreCase)
                || (turnoDateTime.HasValue && turnoDateTime.Value <= DateTime.Now))
            {
                TempData["Mensaje"] = "No se puede editar un turno que está marcado como 'Culminado' o cuya fecha ya pasó.";
                return RedirectToAction(nameof(Main));
            }

            // Mostrar vista Edit con el DTO existente
            return View(shift);
        }

        /// <summary>
        /// POST: Editar turno
        /// Actualiza solo los campos editables del paciente y mensaje. Revalida condiciones.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(ShiftDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var existing = await _shiftService.GetByIdAsync(dto.Id);
            if (existing == null) return NotFound();

            // Recalcular fecha/hora completa del turno actual
            DateTime? turnoDateTime = null;
            if (existing.Date.HasValue && !string.IsNullOrEmpty(existing.ScheduleHour)
                && TimeSpan.TryParse(existing.ScheduleHour, out var shiftTime))
            {
                turnoDateTime = existing.Date.Value.Date.Add(shiftTime);
            }

            // Denegar edición si el turno está "Culminado" O si la fecha+hora ya pasó.
            if (string.Equals(existing.ShiftStateId, ShiftStateEnum.Culminado.ToString(), StringComparison.OrdinalIgnoreCase)
                || (turnoDateTime.HasValue && turnoDateTime.Value <= DateTime.Now))
            {
                TempData["Mensaje"] = "No se puede editar un turno que está marcado como 'Culminado' o cuya fecha ya pasó.";
                return RedirectToAction(nameof(Main));
            }

            try
            {
                // Actualizar únicamente campos permitidos para evitar sobrescribir propiedades no enviadas
                existing.FirstName = dto.FirstName;
                existing.LastName = dto.LastName;
                existing.Email = dto.Email;
                existing.Phone = dto.Phone;
                existing.Message = dto.Message;

                await _shiftService.UpdateAsync(existing);

                TempData["Mensaje"] = "Turno actualizado correctamente.";
                return RedirectToAction(nameof(Main));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        /// <summary>
        /// Eliminar un turno (solo administradores)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _shiftService.DeleteAsync(id);
                TempData["Mensaje"] = "Turno eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        /// <summary>
        /// Aprobar un turno pendiente asignando un profesional
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Approve(int id, int professionalId)
        {
            try
            {
                await _shiftService.ApproveAsync(id, professionalId);
                TempData["Mensaje"] = "Turno aprobado y profesional asignado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al aprobar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        /// <summary>
        /// Rechazar un turno pendiente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                await _shiftService.RejectAsync(id);
                TempData["Mensaje"] = "Turno rechazado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al rechazar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        /// <summary>
        /// Cancelar un turno confirmado
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _shiftService.CancelAsync(id);
                TempData["Mensaje"] = "Turno cancelado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al cancelar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        /// <summary>
        /// Marcar un turno como culminado
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _shiftService.CompleteAsync(id);
                TempData["Mensaje"] = "Turno marcado como culminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al culminar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        #endregion
    }
}
