using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Web.Models
{
    public class DashboardIndexViewModel
    {
        public int PendingCount { get; set; }
        public int TodayCount { get; set; }
        public int ActiveProfessionalsCount { get; set; }
        public int UpcomingHolidaysCount { get; set; }

        public IEnumerable<ShiftDTO> PendingShifts { get; set; } = new List<ShiftDTO>();
        public int PendingTotal { get; set; }
        public int PendingCurrentPage { get; set; }

        public IEnumerable<ShiftDTO> TodayShifts { get; set; } = new List<ShiftDTO>();
        public int TodayTotal { get; set; }
        public int TodayCurrentPage { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime TodayDate { get; set; }

        public int? TodayCurrentShiftId { get; set; }
        public int? TodayNextShiftId { get; set; }
        public ISet<int> UrgentPendingShiftIds { get; set; } = new HashSet<int>();

        public IEnumerable<ContactMessageDTO> ContactMessages { get; set; } = new List<ContactMessageDTO>();
        public int UnansweredMessagesCount => ContactMessages is ICollection<ContactMessageDTO> c ? c.Count : (ContactMessages?.Count() ?? 0);
    }
}
