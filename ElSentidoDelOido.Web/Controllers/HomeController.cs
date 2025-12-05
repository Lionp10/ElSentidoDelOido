using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ElSentidoDelOido.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContactMessageService _contactService;
        private readonly IGoogleRecaptchaService _recaptchaService;

        public HomeController(
            ILogger<HomeController> logger, 
            IContactMessageService contactService,
            IGoogleRecaptchaService recaptchaService)
        {
            _logger = logger;
            _contactService = contactService;
            _recaptchaService = recaptchaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarFormulario(ContactFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["FormError"] = "Por favor corrige los errores del formulario.";
                return RedirectToAction("Index");
            }

            // Verificar reCAPTCHA
            var isRecaptchaValid = await _recaptchaService.VerifyTokenAsync(model.RecaptchaToken);
            if (!isRecaptchaValid)
            {
                _logger.LogWarning("Intento de envío de formulario con reCAPTCHA inválido desde {IP}", HttpContext.Connection.RemoteIpAddress);
                TempData["FormError"] = "Verificación de seguridad fallida. Por favor, intenta nuevamente.";
                return RedirectToAction("Index");
            }

            try
            {
                var createDto = new ContactMessageCreateDTO
                {
                    FullName = model.NombreCompleto,
                    Email = model.Email,
                    Phone = model.Telefono,
                    Message = model.Mensaje
                };

                await _contactService.CreateAsync(createDto);

                TempData["FormSuccess"] = "Mensaje enviado correctamente. ¡Gracias!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar mensaje de contacto");
                TempData["FormError"] = "Ocurrió un error enviando el mensaje. Intenta nuevamente más tarde.";
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
