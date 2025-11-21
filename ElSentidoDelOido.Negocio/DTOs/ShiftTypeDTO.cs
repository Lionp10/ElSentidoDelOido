using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class ShiftTypeDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool? Enabled { get; set; }
    }

    public class ShiftTypeCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string? Name { get; set; }

        public bool Enabled { get; set; } = true;
    }

    public class ShiftTypeUpdateDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string? Name { get; set; }

        public bool Enabled { get; set; }
    }
}
