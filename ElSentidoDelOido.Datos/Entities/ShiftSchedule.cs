using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Entities
{
    public class ShiftSchedule
    {
        public int Id { get; set; }

        public string? Hour { get; set; }

        public int? ShiftTypeId { get; set; }

        public ShiftType? ShiftType { get; set; }

        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}
