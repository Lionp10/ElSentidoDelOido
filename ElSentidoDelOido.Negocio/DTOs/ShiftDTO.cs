using System;
using System.ComponentModel.DataAnnotations;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class ShiftDTO
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? Date { get; set; }
        public int? ScheduleId { get; set; }
        public string? Message { get; set; }
        public int? ShiftTypeId { get; set; }
        public string? ShiftStateId { get; set; }
        public int? ProfessionalId { get; set; }
        
        // Propiedades navegacionales
        public string? ShiftTypeName { get; set; }
        public string? ScheduleHour { get; set; }
        public string? ProfessionalName { get; set; }
    }

    public class ShiftCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un horario")]
        public string? Hour { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de turno")]
        public int TipoTurnoId { get; set; }

        [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
        public string? Message { get; set; }
    }
}
