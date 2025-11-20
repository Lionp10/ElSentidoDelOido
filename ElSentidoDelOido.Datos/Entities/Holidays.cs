using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Entities
{
    public class Holidays
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ShiftTypeId { get; set; }
        public string? Message { get; set; }

        public virtual ShiftType? ShiftType { get; set; }
    }
}
