using System.ComponentModel.DataAnnotations;

namespace ElSentidoDelOido.Web.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "NombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(4000, ErrorMessage = "El mensaje es demasiado largo")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
