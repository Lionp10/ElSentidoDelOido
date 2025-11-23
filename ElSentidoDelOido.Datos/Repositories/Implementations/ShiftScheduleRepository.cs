using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Entities.Enums;
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
                .Include(s => s.ShiftType)
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

        public async Task<IEnumerable<Shift>> GetShiftsByTypeAndDateWithBlockingStatesAsync(int shiftTypeId, DateTime date)
        {
            var dateOnly = date.Date;

            var blockingStates = new[]
            {
                ShiftStateEnum.Pendiente.ToString(),
                ShiftStateEnum.Confirmado.ToString()
            };

            return await _context.Shifts
                .Where(s => s.ShiftTypeId == shiftTypeId
                            && s.Date.HasValue
                            && s.Date.Value.Date == dateOnly
                            && s.ShiftStateId != null
                            && blockingStates.Contains(s.ShiftStateId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftSchedule>> GetAllAsync()
        {
            return await _context.ShiftSchedules
                .Include(s => s.ShiftType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ShiftSchedule?> GetByIdAsync(int id)
        {
            return await _context.ShiftSchedules
                .Include(s => s.ShiftType)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ShiftSchedule> CreateAsync(ShiftSchedule entity)
        {
            _context.ShiftSchedules.Add(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task<ShiftSchedule> UpdateAsync(ShiftSchedule entity)
        {
            _context.ShiftSchedules.Update(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ShiftSchedules.FindAsync(id);
            if (entity == null) return;
            _context.ShiftSchedules.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByHourAndTypeAsync(string hour, int shiftTypeId, int? excludeId = null)
        {
            var query = _context.ShiftSchedules
                .Where(s => s.ShiftTypeId == shiftTypeId && s.Hour == hour);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AsNoTracking().AnyAsync();
        }
    }
}
