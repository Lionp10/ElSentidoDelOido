using ElSentidoDelOido.Datos.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Entities
{
    public class Shift
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

        public virtual Professional? Professional { get; set; }

        public virtual ShiftSchedule? Schedule { get; set; }

        public virtual ShiftStateEnum? ShiftState 
        { 
            get 
            {
                if (string.IsNullOrEmpty(ShiftStateId))
                    return null;
                
                if (Enum.TryParse<ShiftStateEnum>(ShiftStateId, out var result))
                    return result;
                
                return null;
            }
            set
            {
                ShiftStateId = value?.ToString();
            }
        }

        public virtual ShiftType? ShiftType { get; set; }
    }
}
