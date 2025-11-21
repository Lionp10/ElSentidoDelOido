using System.ComponentModel.DataAnnotations;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class ShiftScheduleDTO
    {
        public int Id { get; set; }
        public string? Hour { get; set; }      
        public bool Enabled { get; set; }  

        public int? ShiftTypeId { get; set; }
        public string? ShiftTypeName { get; set; }
    }

    public class ShiftScheduleCreateDTO
    {
        [Required(ErrorMessage = "El horario es obligatorio.")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Formato de hora inválido. Use HH:mm (ej. 14:30).")]
        public string? Hour { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de turno.")]
        public int? ShiftTypeId { get; set; }
    }

    public class ShiftScheduleUpdateDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El horario es obligatorio.")]
        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Formato de hora inválido. Use HH:mm (ej. 14:30).")]
        public string? Hour { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de turno.")]
        public int? ShiftTypeId { get; set; }
    }
}
