using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class ShiftTypeRepository : IShiftTypeRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public ShiftTypeRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShiftType>> GetAllAsync()
        {
            return await _context.ShiftTypes
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ShiftType?> GetByIdAsync(int id)
        {
            return await _context.ShiftTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ShiftType> CreateAsync(ShiftType entity)
        {
            _context.ShiftTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ShiftType> UpdateAsync(ShiftType entity)
        {
            _context.ShiftTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ShiftTypes.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return;
            entity.Enabled = false;
            _context.ShiftTypes.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int id)
        {
            var entity = await _context.ShiftTypes.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return;
            entity.Enabled = true;
            _context.ShiftTypes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
