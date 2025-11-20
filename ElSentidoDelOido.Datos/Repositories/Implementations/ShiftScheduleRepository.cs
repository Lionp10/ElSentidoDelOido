using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class ShiftScheduleRepository : IShiftScheduleRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public ShiftScheduleRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShiftSchedule>> GetByShiftTypeAsync(int shiftTypeId)
        {
            return await _context.ShiftSchedules
                .Where(s => s.ShiftTypeId == shiftTypeId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Shift>> GetShiftsByTypeAndDateAsync(int shiftTypeId, DateTime date)
        {
            var dateOnly = date.Date;
            return await _context.Shifts
                .Where(s => s.ShiftTypeId == shiftTypeId
                            && s.Date.HasValue
                            && s.Date.Value.Date == dateOnly)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
