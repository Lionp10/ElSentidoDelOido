using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class HolidaysRepository : IHolidaysRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public HolidaysRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Holidays>> GetAllAsync()
        {
            return await _context.Holidays
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Holidays?> GetByIdAsync(int id)
        {
            return await _context.Holidays
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<Holidays> CreateAsync(Holidays entity)
        {
            _context.Holidays.Add(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task<Holidays> UpdateAsync(Holidays entity)
        {
            _context.Holidays.Update(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Holidays.FindAsync(id);
            if (entity == null) return;
            _context.Holidays.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByDateAsync(DateTime date, int? excludeId = null)
        {
            var target = date.Date;
            return await _context.Holidays
                .AsNoTracking()
                .AnyAsync(h => h.Date.Date == target && (!excludeId.HasValue || h.Id != excludeId.Value));
        }
    }
}
