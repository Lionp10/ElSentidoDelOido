using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class ShiftTypeDTO
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public bool? Enabled { get; set; }
    }
}
