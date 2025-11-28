using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Helpers;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Encodings.Web;
using MimeKit;
using MailKit.Net.Smtp;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly IContactMessageService _contactService;
        private readonly IEmailService _emailService;
        private readonly IRazorViewToStringRenderer _viewRenderer;
        private readonly ILogger<ContactController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ContactController(
            IContactMessageService contactService, 
            IEmailService emailService, 
            IRazorViewToStringRenderer viewRenderer, 
            ILogger<ContactController> logger,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _contactService = contactService;
            _emailService = emailService;
            _viewRenderer = viewRenderer;
            _logger = logger;
            _configuration = configuration;
            _env = env;
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

            var msg = await _contactService.GetByIdAsync(id);
            if (msg == null) return NotFound();

            try
            {
                await _contactService.ReplyAsync(id, reply);
                TempData["Mensaje"] = "Respuesta registrada correctamente.";

                var msgUpdated = await _contactService.GetByIdAsync(id) ?? msg;

                if (!string.IsNullOrWhiteSpace(msgUpdated.Email))
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

                        var safeContactName = HtmlEncoder.Default.Encode(msgUpdated.FullName ?? "Contacto");
                        var safeOriginalMessage = HtmlEncoder.Default.Encode(msgUpdated.Message ?? string.Empty).Replace("\n", "<br/>");
                        var safeReplyMessage = HtmlEncoder.Default.Encode(reply).Replace("\n", "<br/>");
                        var replyDate = msgUpdated.RepliedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") 
                                     ?? DateTime.UtcNow.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

                        var subject = "Respuesta a tu mensaje - El Sentido del Oído";

                        var emailModel = new ElSentidoDelOido.Web.Models.ContactReplyEmailViewModel
                        {
                            ContactName = safeContactName,
                            OriginalMessage = safeOriginalMessage,
                            ReplyMessage = safeReplyMessage,
                            ReplyDate = replyDate,
                            BaseUrl = baseUrl,
                            LogoDataUri = "cid:logo"
                        };

                        var body = await _viewRenderer.RenderViewToStringAsync("~/Views/Emails/ContactReply.cshtml", emailModel);

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(
                            _configuration["SmtpProfiles:contact:FromName"] ?? _configuration["Smtp:FromName"] ?? "El Sentido del Oído",
                            _configuration["SmtpProfiles:contact:FromEmail"] ?? _configuration["Smtp:FromEmail"] ?? "contacto@elsentidodeloido.com"));
                        message.To.Add(new MailboxAddress(msgUpdated.FullName ?? "Contacto", msgUpdated.Email));
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

                        var smtpHost = _configuration["SmtpProfiles:contact:Host"] ?? _configuration["Smtp:Host"];
                        var smtpPortStr = _configuration["SmtpProfiles:contact:Port"] ?? _configuration["Smtp:Port"] ?? "587";
                        var smtpPort = int.Parse(smtpPortStr);
                        var smtpUser = _configuration["SmtpProfiles:contact:User"] ?? _configuration["Smtp:User"];
                        var smtpPass = _configuration["SmtpProfiles:contact:Pass"] ?? _configuration["Smtp:Pass"];
                        var useSslStr = _configuration["SmtpProfiles:contact:UseSsl"] ?? _configuration["Smtp:UseSsl"] ?? "false";
                        var useSsl = bool.Parse(useSslStr);

                        using var client = new SmtpClient();
                        
                        var secureSocketOptions = useSsl && smtpPort == 465 
                            ? MailKit.Security.SecureSocketOptions.SslOnConnect 
                            : MailKit.Security.SecureSocketOptions.StartTls;
                        
                        await client.ConnectAsync(smtpHost, smtpPort, secureSocketOptions);
                        await client.AuthenticateAsync(smtpUser, smtpPass);
                        await client.SendAsync(message);
                        await client.DisconnectAsync(true);

                        _logger.LogInformation("Email de respuesta enviado exitosamente para el mensaje {Id}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "No se pudo enviar el email de respuesta para el mensaje {Id}", id);
                        TempData["Mensaje"] = "Respuesta registrada, pero no se pudo enviar el correo al cliente.";
                    }
                }
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
