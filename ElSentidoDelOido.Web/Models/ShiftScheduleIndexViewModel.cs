using ElSentidoDelOido.Negocio.DTOs;
using System.Collections.Generic;

namespace ElSentidoDelOido.Web.Models
{
    public class ShiftScheduleIndexViewModel
    {
        public IEnumerable<ShiftScheduleDTO> Items { get; set; } = Enumerable.Empty<ShiftScheduleDTO>();
        public IEnumerable<ShiftTypeDTO> ShiftTypes { get; set; } = Enumerable.Empty<ShiftTypeDTO>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int? SelectedShiftTypeId { get; set; }
    }
}
