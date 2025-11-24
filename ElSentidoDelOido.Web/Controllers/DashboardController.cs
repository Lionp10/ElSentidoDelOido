using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ElSentidoDelOido.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IShiftService _shiftService;
        private readonly IProfessionalService _professionalService;
        private readonly IHolidaysService _holidaysService;
        private readonly IContactMessageService _contactService;

        public DashboardController(
            IShiftService shiftService,
            IProfessionalService professionalService,
            IHolidaysService holidaysService,
            IContactMessageService contactService)
        {
            _shiftService = shiftService;
            _professionalService = professionalService;
            _holidaysService = holidaysService;
            _contactService = contactService;
        }

        public async Task<IActionResult> Index(int pendingPage = 1, int todayPage = 1, int pageSize = 10)
        {
            pendingPage = Math.Max(1, pendingPage);
            todayPage = Math.Max(1, todayPage);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var (pendingItems, pendingTotal) = await _shiftService.GetPagedAsync(
                fecha: null,
                estado: "Pendiente",
                tipoTurnoId: null,
                professionalId: null,
                page: pendingPage,
                pageSize: pageSize);

            var today = DateTime.Today;
            var (todayItemsAll, todayTotalAll) = await _shiftService.GetPagedAsync(
                fecha: today,
                estado: null,
                tipoTurnoId: null,
                professionalId: null,
                page: 1,
                pageSize: int.MaxValue); 

            var todayConfirmed = todayItemsAll.Where(s => string.Equals(s.ShiftStateId, "Confirmado", StringComparison.OrdinalIgnoreCase))
                                             .OrderBy(s =>
                                             {
                                                 if (TimeSpan.TryParseExact(s.ScheduleHour ?? string.Empty, @"hh\:mm", CultureInfo.InvariantCulture, out var t) 
                                                     || TimeSpan.TryParse(s.ScheduleHour ?? string.Empty, out t))
                                                 {
                                                     return t;
                                                 }
                                                 return TimeSpan.MaxValue;
                                             })
                                             .ToList();

            DateTime now = DateTime.Now;
            int? currentId = null;
            int? nextId = null;

            var mappedTodayTimes = new List<(int Id, DateTime Dt)>();
            foreach (var s in todayConfirmed)
            {
                if (!s.Date.HasValue || string.IsNullOrEmpty(s.ScheduleHour)) continue;

                if (TimeSpan.TryParseExact(s.ScheduleHour, @"hh\:mm", CultureInfo.InvariantCulture, out var ts)
                    || TimeSpan.TryParse(s.ScheduleHour, out ts))
                {
                    var shiftDt = s.Date.Value.Date.Add(ts);
                    mappedTodayTimes.Add((s.Id, shiftDt));
                }
            }

            mappedTodayTimes = mappedTodayTimes.OrderBy(x => x.Dt).ToList();

            nextId = mappedTodayTimes.FirstOrDefault(x => x.Dt > now).Id == 0 ? null : (int?)mappedTodayTimes.FirstOrDefault(x => x.Dt > now).Id;
            var lastPast = mappedTodayTimes.LastOrDefault(x => x.Dt <= now);
            if (lastPast != default)
            {
                if ((now - lastPast.Dt) <= TimeSpan.FromHours(1))
                {
                    currentId = lastPast.Id;
                }
            }

            var urgentSet = new HashSet<int>();
            foreach (var s in pendingItems)
            {
                if (!s.Date.HasValue || string.IsNullOrEmpty(s.ScheduleHour)) continue;

                if (TimeSpan.TryParseExact(s.ScheduleHour, @"hh\:mm", CultureInfo.InvariantCulture, out var ts)
                    || TimeSpan.TryParse(s.ScheduleHour, out ts))
                {
                    var shiftDt = s.Date.Value.Date.Add(ts);
                    var diff = shiftDt - now;
                    if (diff.TotalHours >= 0 && diff.TotalHours <= 24)
                    {
                        urgentSet.Add(s.Id);
                    }
                }
            }

            var totalTodayConfirmed = todayConfirmed.Count;
            var todayPaged = todayConfirmed
                .Skip((todayPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var professionals = await _professionalService.GetAllAsync();
            var activeProfessionalsCount = professionals.Count(p => p.Enabled.GetValueOrDefault());

            var feriados = (await _holidaysService.GetAllAsync()).ToList();
            var upcomingHolidays = feriados.Where(f => f.Date.Date >= today && f.Date.Date <= today.AddDays(30)).ToList();

            var allMessages = (await _contactService.GetAllAsync()) ?? Enumerable.Empty<ContactMessageDTO>();
            var unanswered = allMessages.Where(m => !m.Answered).OrderByDescending(m => m.CreatedAt).ToList();

            var model = new DashboardIndexViewModel
            {
                PendingCount = pendingTotal,
                TodayCount = totalTodayConfirmed,
                ActiveProfessionalsCount = activeProfessionalsCount,
                UpcomingHolidaysCount = upcomingHolidays.Count,

                PendingShifts = pendingItems,
                PendingTotal = pendingTotal,
                PendingCurrentPage = pendingPage,

                TodayShifts = todayPaged,
                TodayTotal = totalTodayConfirmed,
                TodayCurrentPage = todayPage,

                PageSize = pageSize,
                TodayDate = today,

                TodayCurrentShiftId = currentId,
                TodayNextShiftId = nextId,
                UrgentPendingShiftIds = urgentSet,

                ContactMessages = unanswered
            };

            return View(model);
        }
    }
}
