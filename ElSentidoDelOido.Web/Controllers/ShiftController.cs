using ElSentidoDelOido.Datos.Entities.Enums;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Helpers;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using MimeKit;
using MailKit.Net.Smtp;

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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IRazorViewToStringRenderer _viewRenderer;

        public ShiftController(
            IShiftTypeService shiftTypeService, 
            IShiftScheduleService shiftScheduleService, 
            IHolidaysService holidaysService,
            IShiftService shiftService,
            IProfessionalService professionalService,
            IEmailService emailService,
            IRazorViewToStringRenderer viewRenderer,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _shiftTypeService = shiftTypeService;
            _shiftScheduleService = shiftScheduleService;
            _holidaysService = holidaysService;
            _shiftService = shiftService;
            _professional_service = professionalService;
            _emailService = emailService;
            _viewRenderer = viewRenderer;
            _configuration = configuration;
            _env = env;
        }

        #endregion

        #region Público - Solicitud de Turnos
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

            var (items, totalCount) = await _shiftService.GetPagedAsync(fecha, estado, tipoTurnoId, professionalId, page, pageSize);

            var model = new ShiftMainViewModel
            {
                Items = items,
                FechaFiltro = fecha,
                EstadoFiltro = estado,
                TipoTurnoFiltro = tipoTurnoId,
                ProfessionalFiltro = professionalId,
                ShiftTypes = shiftTypes,
                Professionals = professionals.Where(p => p.Enabled.GetValueOrDefault()),
                EstadosDisponibles = estadosDisponibles,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();

            DateTime? turnoDateTime = null;
            if (shift.Date.HasValue && !string.IsNullOrEmpty(shift.ScheduleHour)
                && TimeSpan.TryParse(shift.ScheduleHour, out var shiftTime))
            {
                turnoDateTime = shift.Date.Value.Date.Add(shiftTime);
            }

            if (string.Equals(shift.ShiftStateId, ShiftStateEnum.Culminado.ToString(), StringComparison.OrdinalIgnoreCase)
                || (turnoDateTime.HasValue && turnoDateTime.Value <= DateTime.Now))
            {
                TempData["Mensaje"] = "No se puede editar un turno que está marcado como 'Culminado' o cuya fecha ya pasó.";
                return RedirectToAction(nameof(Main));
            }

            return View(shift);
        }

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

            DateTime? turnoDateTime = null;
            if (existing.Date.HasValue && !string.IsNullOrEmpty(existing.ScheduleHour)
                && TimeSpan.TryParse(existing.ScheduleHour, out var shiftTime))
            {
                turnoDateTime = existing.Date.Value.Date.Add(shiftTime);
            }

            if (string.Equals(existing.ShiftStateId, ShiftStateEnum.Culminado.ToString(), StringComparison.OrdinalIgnoreCase)
                || (turnoDateTime.HasValue && turnoDateTime.Value <= DateTime.Now))
            {
                TempData["Mensaje"] = "No se puede editar un turno que está marcado como 'Culminado' o cuya fecha ya pasó.";
                return RedirectToAction(nameof(Main));
            }

            try
            {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Approve(int id, int professionalId)
        {
            try
            {
                await _shiftService.ApproveAsync(id, professionalId);

                var shift = await _shiftService.GetByIdAsync(id);
                var professionals = await _professional_service.GetAllAsync();
                var profesional = professionals.FirstOrDefault(p => p.Id == professionalId);

                if (shift != null && !string.IsNullOrWhiteSpace(shift.Email))
                {
                    try
                    {
                        var configuredBase = _configuration["App:BaseUrl"];
                        string baseUrl;
                        
                        if (!string.IsNullOrWhiteSpace(configuredBase))
                        {
                            baseUrl = configuredBase.TrimEnd('/');
                        }
                        else
                        {
                            var scheme = Request.Scheme ?? "https";
                            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                            baseUrl = $"{scheme}://{host}";
                        }

                        var clinicAddress = profesional?.Address ?? _configuration["Clinic:Address"] ?? "Belgrano 1212, Arequito, Santa Fe";
                        var patientFullName = $"{shift.FirstName} {shift.LastName}".Trim();
                        var safePatientName = HtmlEncoder.Default.Encode(patientFullName);
                        var safeProfessionalName = HtmlEncoder.Default.Encode(
                            profesional != null ? $"{profesional.FirstName} {profesional.LastName}".Trim() : "Profesional asignado");
                        var safeType = HtmlEncoder.Default.Encode(shift.ShiftTypeName ?? "-");
                        var safeDate = shift.Date.HasValue ? shift.Date.Value.ToString("dd/MM/yyyy") : "-";
                        var safeHour = HtmlEncoder.Default.Encode(shift.ScheduleHour ?? "-");
                        var patientNoteHtml = HtmlEncoder.Default.Encode(shift.Message ?? string.Empty).Replace("\n", "<br/>");

                        var subject = $"Turno confirmado - {safeDate} {safeHour}";

                        var emailModel = new ShiftEmailViewModel
                        {
                            PatientName = safePatientName,
                            Date = safeDate,
                            Hour = safeHour,
                            ShiftType = safeType,
                            Professional = safeProfessionalName,
                            ClinicAddress = HtmlEncoder.Default.Encode(clinicAddress),
                            PatientNote = patientNoteHtml,
                            BaseUrl = baseUrl,
                            LogoDataUri = "cid:logo"
                        };

                        var body = await _viewRenderer.RenderViewToStringAsync("~/Views/Emails/ShiftApproved.cshtml", emailModel);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(
                            _configuration["SmtpProfiles:notifications:FromName"] ?? _configuration["Smtp:FromName"] ?? "El Sentido del Oído",
                            _configuration["SmtpProfiles:notifications:FromEmail"] ?? _configuration["Smtp:FromEmail"] ?? "info@elsentidodeloido.com"));
                        message.To.Add(new MailboxAddress(patientFullName, shift.Email));
                        message.Subject = subject;

                        var builder = new BodyBuilder { HtmlBody = body };

                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var logoPath = Path.Combine(webRoot, "Images", "LogoOido.png");
                        
                        if (!System.IO.File.Exists(logoPath))
                        {
                            logoPath = Path.Combine(webRoot, "Images", "LogoOido.webp");
                        }

                        if (System.IO.File.Exists(logoPath))
                        {
                            var logo = builder.LinkedResources.Add(logoPath);
                            logo.ContentId = "logo";
                        }

                        message.Body = builder.ToMessageBody();

                        var smtpHost = _configuration["SmtpProfiles:notifications:Host"] ?? _configuration["Smtp:Host"];
                        var smtpPortStr = _configuration["SmtpProfiles:notifications:Port"] ?? _configuration["Smtp:Port"] ?? "587";
                        var smtpPort = int.Parse(smtpPortStr);
                        var smtpUser = _configuration["SmtpProfiles:notifications:User"] ?? _configuration["Smtp:User"];
                        var smtpPass = _configuration["SmtpProfiles:notifications:Pass"] ?? _configuration["Smtp:Pass"];
                        var useSslStr = _configuration["SmtpProfiles:notifications:UseSsl"] ?? _configuration["Smtp:UseSsl"] ?? "false";
                        var useSsl = bool.Parse(useSslStr);

                        using var client = new SmtpClient();
                        
                        var secureSocketOptions = useSsl && smtpPort == 465 
                            ? MailKit.Security.SecureSocketOptions.SslOnConnect 
                            : MailKit.Security.SecureSocketOptions.StartTls;
                        
                        await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al enviar email de confirmación: {mailEx}");
                    }
                }

                TempData["Mensaje"] = "Turno aprobado y profesional asignado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al aprobar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            try
            {
                await _shiftService.RejectAsync(id);

                var shift = await _shiftService.GetByIdAsync(id);

                if (shift != null && !string.IsNullOrWhiteSpace(shift.Email))
                {
                    try
                    {
                        var configuredBase = _configuration["App:BaseUrl"];
                        string baseUrl;
                        
                        if (!string.IsNullOrWhiteSpace(configuredBase))
                        {
                            baseUrl = configuredBase.TrimEnd('/');
                        }
                        else
                        {
                            var scheme = Request.Scheme ?? "https";
                            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                            baseUrl = $"{scheme}://{host}";
                        }

                        var clinicAddress = _configuration["Clinic:Address"] ?? "Belgrano 1212, Arequito, Santa Fe";
                        var patientFullName = $"{shift.FirstName} {shift.LastName}".Trim();
                        var safePatientName = HtmlEncoder.Default.Encode(patientFullName);
                        var safeType = HtmlEncoder.Default.Encode(shift.ShiftTypeName ?? "-");
                        var safeDate = shift.Date.HasValue ? shift.Date.Value.ToString("dd/MM/yyyy") : "-";
                        var safeHour = HtmlEncoder.Default.Encode(shift.ScheduleHour ?? "-");
                        var safeRejectionReason = HtmlEncoder.Default.Encode(rejectionReason ?? "No se especificó un motivo").Replace("\n", "<br/>");

                        var subject = $"Turno rechazado - {safeDate} {safeHour}";

                        var emailModel = new ShiftRejectedEmailViewModel
                        {
                            PatientName = safePatientName,
                            Date = safeDate,
                            Hour = safeHour,
                            ShiftType = safeType,
                            RejectionReason = safeRejectionReason,
                            ClinicAddress = HtmlEncoder.Default.Encode(clinicAddress),
                            BaseUrl = baseUrl,
                            LogoDataUri = "cid:logo"
                        };

                        var body = await _viewRenderer.RenderViewToStringAsync("~/Views/Emails/ShiftRejected.cshtml", emailModel);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(
                            _configuration["SmtpProfiles:notifications:FromName"] ?? _configuration["Smtp:FromName"] ?? "El Sentido del Oído",
                            _configuration["SmtpProfiles:notifications:FromEmail"] ?? _configuration["Smtp:FromEmail"] ?? "info@elsentidodeloido.com"));
                        message.To.Add(new MailboxAddress(patientFullName, shift.Email));
                        message.Subject = subject;

                        var builder = new BodyBuilder { HtmlBody = body };

                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var logoPath = Path.Combine(webRoot, "Images", "LogoOido.png");
                        
                        if (!System.IO.File.Exists(logoPath))
                        {
                            logoPath = Path.Combine(webRoot, "Images", "LogoOido.webp");
                        }

                        if (System.IO.File.Exists(logoPath))
                        {
                            var logo = builder.LinkedResources.Add(logoPath);
                            logo.ContentId = "logo";
                        }

                        message.Body = builder.ToMessageBody();

                        var smtpHost = _configuration["SmtpProfiles:notifications:Host"] ?? _configuration["Smtp:Host"];
                        var smtpPortStr = _configuration["SmtpProfiles:notifications:Port"] ?? _configuration["Smtp:Port"] ?? "587";
                        var smtpPort = int.Parse(smtpPortStr);
                        var smtpUser = _configuration["SmtpProfiles:notifications:User"] ?? _configuration["Smtp:User"];
                        var smtpPass = _configuration["SmtpProfiles:notifications:Pass"] ?? _configuration["Smtp:Pass"];
                        var useSslStr = _configuration["SmtpProfiles:notifications:UseSsl"] ?? _configuration["Smtp:UseSsl"] ?? "false";
                        var useSsl = bool.Parse(useSslStr);

                        using var client = new SmtpClient();
                        
                        var secureSocketOptions = useSsl && smtpPort == 465 
                            ? MailKit.Security.SecureSocketOptions.SslOnConnect 
                            : MailKit.Security.SecureSocketOptions.StartTls;
                        
                        await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al enviar email de rechazo: {mailEx}");
                    }
                }

                TempData["Mensaje"] = "Turno rechazado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al rechazar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id, string cancellationReason)
        {
            try
            {
                await _shiftService.CancelAsync(id);

                var shift = await _shiftService.GetByIdAsync(id);

                if (shift != null && !string.IsNullOrWhiteSpace(shift.Email))
                {
                    try
                    {
                        var configuredBase = _configuration["App:BaseUrl"];
                        string baseUrl;
                        
                        if (!string.IsNullOrWhiteSpace(configuredBase))
                        {
                            baseUrl = configuredBase.TrimEnd('/');
                        }
                        else
                        {
                            var scheme = Request.Scheme ?? "https";
                            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                            baseUrl = $"{scheme}://{host}";
                        }

                        var clinicAddress = _configuration["Clinic:Address"] ?? "Belgrano 1212, Arequito, Santa Fe";
                        var patientFullName = $"{shift.FirstName} {shift.LastName}".Trim();
                        var safePatientName = HtmlEncoder.Default.Encode(patientFullName);
                        var safeType = HtmlEncoder.Default.Encode(shift.ShiftTypeName ?? "-");
                        var safeDate = shift.Date.HasValue ? shift.Date.Value.ToString("dd/MM/yyyy") : "-";
                        var safeHour = HtmlEncoder.Default.Encode(shift.ScheduleHour ?? "-");
                        var safeCancelReason = HtmlEncoder.Default.Encode(cancellationReason ?? "No se especificó un motivo").Replace("\n", "<br/>");

                        var subject = $"Turno cancelado - {safeDate} {safeHour}";

                        var emailModel = new ShiftRejectedEmailViewModel
                        {
                            PatientName = safePatientName,
                            Date = safeDate,
                            Hour = safeHour,
                            ShiftType = safeType,
                            RejectionReason = safeCancelReason, 
                            ClinicAddress = HtmlEncoder.Default.Encode(clinicAddress),
                            BaseUrl = baseUrl,
                            LogoDataUri = "cid:logo"
                        };

                        var body = await _viewRenderer.RenderViewToStringAsync("~/Views/Emails/ShiftCancelled.cshtml", emailModel);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(
                            _configuration["SmtpProfiles:notifications:FromName"] ?? _configuration["Smtp:FromName"] ?? "El Sentido del Oído",
                            "info@elsentidodeloido.com"));
                        message.To.Add(new MailboxAddress(patientFullName, shift.Email));
                        message.Subject = subject;

                        var builder = new BodyBuilder { HtmlBody = body };

                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var logoPath = Path.Combine(webRoot, "Images", "LogoOido.png");
                        
                        if (!System.IO.File.Exists(logoPath))
                        {
                            logoPath = Path.Combine(webRoot, "Images", "LogoOido.webp");
                        }

                        if (System.IO.File.Exists(logoPath))
                        {
                            var logo = builder.LinkedResources.Add(logoPath);
                            logo.ContentId = "logo";
                        }

                        message.Body = builder.ToMessageBody();

                        var smtpHost = _configuration["SmtpProfiles:notifications:Host"] ?? _configuration["Smtp:Host"];
                        var smtpPortStr = _configuration["SmtpProfiles:notifications:Port"] ?? _configuration["Smtp:Port"] ?? "587";
                        var smtpPort = int.Parse(smtpPortStr);
                        var smtpUser = _configuration["SmtpProfiles:notifications:User"] ?? _configuration["Smtp:User"];
                        var smtpPass = _configuration["SmtpProfiles:notifications:Pass"] ?? _configuration["Smtp:Pass"];
                        var useSslStr = _configuration["SmtpProfiles:notifications:UseSsl"] ?? _configuration["Smtp:UseSsl"] ?? "false";
                        var useSsl = bool.Parse(useSslStr);

                        using var client = new SmtpClient();
                        
                        var secureSocketOptions = useSsl && smtpPort == 465 
                            ? MailKit.Security.SecureSocketOptions.SslOnConnect 
                            : MailKit.Security.SecureSocketOptions.StartTls;
                        
                        await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al enviar email de cancelación: {mailEx}");
                    }
                }

                TempData["Mensaje"] = "Turno cancelado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al cancelar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Reprogram(int id, int tipoTurnoId, DateTime date, string hour, int? professionalId)
        {
            try
            {
                var role = User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
                var isAllowed = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(role, "Moderator", StringComparison.OrdinalIgnoreCase);

                if (!isAllowed)
                {
                    return Forbid();
                }

                var existing = await _shiftService.GetByIdAsync(id);
                if (existing == null)
                {
                    TempData["Mensaje"] = "Turno no encontrado.";
                    return RedirectToAction(nameof(Main));
                }

                if (string.Equals(existing.ShiftStateId, ShiftStateEnum.Culminado.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Mensaje"] = "No se puede reprogramar un turno que ya está marcado como 'Culminado'.";
                    return RedirectToAction(nameof(Main));
                }

                int? scheduleId = null;
                try
                {
                    var schedules = await _shiftScheduleService.GetAvailabilityAsync(tipoTurnoId, date.Date);
                    var matched = schedules.FirstOrDefault(s => string.Equals(s.Hour?.Trim(), hour?.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (matched != null)
                    {
                        scheduleId = matched.Id;
                    }
                }
                catch
                {
                    // si falla la búsqueda de horarios seguimos permitiendo actualizar con valor de hora libre
                }

                existing.ShiftTypeId = tipoTurnoId;
                existing.Date = date;
                existing.ScheduleId = scheduleId;
                existing.ScheduleHour = hour;
                existing.ProfessionalId = professionalId;

                await _shiftService.UpdateAsync(existing);

                if (!string.IsNullOrWhiteSpace(existing.Email))
                {
                    try
                    {
                        var configuredBase = _configuration["App:BaseUrl"];
                        string baseUrl;
                        if (!string.IsNullOrWhiteSpace(configuredBase))
                        {
                            baseUrl = configuredBase.TrimEnd('/');
                        }
                        else
                        {
                            var scheme = Request.Scheme ?? "https";
                            var host = Request.Host.HasValue ? Request.Host.Value : "localhost";
                            baseUrl = $"{scheme}://{host}";
                        }

                        var professionals = await _professional_service.GetAllAsync();
                        var profesional = professionalId.HasValue ? professionals.FirstOrDefault(p => p.Id == professionalId.Value) : null;

                        var clinicAddress = profesional?.Address ?? _configuration["Clinic:Address"] ?? "Belgrano 1212, Arequito, Santa Fe";
                        var patientFullName = $"{existing.FirstName} {existing.LastName}".Trim();
                        var safePatientName = HtmlEncoder.Default.Encode(patientFullName);
                        var safeProfessionalName = HtmlEncoder.Default.Encode(profesional != null ? $"{profesional.FirstName} {profesional.LastName}".Trim() : "Profesional asignado");
                        var safeType = HtmlEncoder.Default.Encode(existing.ShiftTypeName ?? "-");
                        var safeDate = existing.Date.HasValue ? existing.Date.Value.ToString("dd/MM/yyyy") : "-";
                        var safeHour = HtmlEncoder.Default.Encode(existing.ScheduleHour ?? hour ?? "-");
                        var patientNoteHtml = HtmlEncoder.Default.Encode(existing.Message ?? string.Empty).Replace("\n", "<br/>");

                        var subject = $"Turno reprogramado - {safeDate} {safeHour}";

                        var emailModel = new ShiftEmailViewModel
                        {
                            PatientName = safePatientName,
                            Date = safeDate,
                            Hour = safeHour,
                            ShiftType = safeType,
                            Professional = safeProfessionalName,
                            ClinicAddress = HtmlEncoder.Default.Encode(clinicAddress),
                            PatientNote = patientNoteHtml,
                            BaseUrl = baseUrl,
                            LogoDataUri = "cid:logo"
                        };

                        var body = await _viewRenderer.RenderViewToStringAsync("~/Views/Emails/ShiftRescheduled.cshtml", emailModel);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(
                            _configuration["SmtpProfiles:notifications:FromName"] ?? _configuration["Smtp:FromName"] ?? "El Sentido del Oído",
                            _configuration["SmtpProfiles:notifications:FromEmail"] ?? _configuration["Smtp:FromEmail"] ?? "info@elsentidodeloido.com"));
                        message.To.Add(new MailboxAddress(patientFullName, existing.Email));
                        message.Subject = subject;

                        var builder = new BodyBuilder { HtmlBody = body };

                        var webRoot = _env.WebRootPath ?? string.Empty;
                        var logoPath = Path.Combine(webRoot, "Images", "LogoOido.png");
                        if (!System.IO.File.Exists(logoPath))
                        {
                            logoPath = Path.Combine(webRoot, "Images", "LogoOido.webp");
                        }

                        if (System.IO.File.Exists(logoPath))
                        {
                            var logo = builder.LinkedResources.Add(logoPath);
                            logo.ContentId = "logo";
                        }

                        message.Body = builder.ToMessageBody();

                        var smtpHost = _configuration["SmtpProfiles:notifications:Host"] ?? _configuration["Smtp:Host"];
                        var smtpPortStr = _configuration["SmtpProfiles:notifications:Port"] ?? _configuration["Smtp:Port"] ?? "587";
                        var smtpPort = int.Parse(smtpPortStr);
                        var smtpUser = _configuration["SmtpProfiles:notifications:User"] ?? _configuration["Smtp:User"];
                        var smtpPass = _configuration["SmtpProfiles:notifications:Pass"] ?? _configuration["Smtp:Pass"];
                        var useSslStr = _configuration["SmtpProfiles:notifications:UseSsl"] ?? _configuration["Smtp:UseSsl"] ?? "false";
                        var useSsl = bool.Parse(useSslStr);

                        using var client = new MailKit.Net.Smtp.SmtpClient();
                        var secureSocketOptions = useSsl && smtpPort == 465
                            ? MailKit.Security.SecureSocketOptions.SslOnConnect
                            : MailKit.Security.SecureSocketOptions.StartTls;

                        await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al enviar email de reprogramación: {mailEx}");
                    }
                }

                TempData["Mensaje"] = "Turno reprogramado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al reprogramar el turno: {ex.Message}";
            }

            return RedirectToAction(nameof(Main));
        }

        #endregion

        #region Helpers

        private string? GetLogoDataUri()
        {
            try
            {
                var webRoot = _env.WebRootPath ?? string.Empty;
                var pngPath = Path.Combine(webRoot, "Images", "LogoOido.png");
                var webpPath = Path.Combine(webRoot, "Images", "LogoOido.webp");

                string? path = null;
                string mimeType;

                if (System.IO.File.Exists(pngPath))
                {
                    path = pngPath;
                    mimeType = "image/png";
                }
                else if (System.IO.File.Exists(webpPath))
                {
                    path = webpPath;
                    mimeType = "image/webp";
                }
                else
                {
                    return null;
                }

                var bytes = System.IO.File.ReadAllBytes(path);
                var base64 = Convert.ToBase64String(bytes);
                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo leer logo para data-uri: {ex}");
                return null;
            }
        }

        #endregion
    }
}
