using System.ComponentModel.DataAnnotations;

namespace ElSentidoDelOido.Web.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Teléfono inválido")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verificación de reCAPTCHA requerida")]
        public string RecaptchaToken { get; set; } = string.Empty;
    }
}
