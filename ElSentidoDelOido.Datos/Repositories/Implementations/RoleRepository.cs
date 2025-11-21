using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public RoleRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetActiveAsync()
        {
            // Evitar error de conversión entre bool? y bool en el lambda:
            return await _context.UserRoles
                .Where(r => r.Enabled.HasValue && r.Enabled.Value)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserRole?> GetByIdAsync(int id)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
