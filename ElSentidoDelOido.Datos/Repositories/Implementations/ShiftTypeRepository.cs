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
    }
}
