using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class ProfessionalRepository : IProfessionalRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public ProfessionalRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Professional>> GetAllAsync()
        {
            return await _context.Professionals
                .Where(p => p.Enabled == true)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Professional?> GetByIdAsync(int id)
        {
            return await _context.Professionals
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Professional> CreateAsync(Professional entity)
        {
            _context.Professionals.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Professional> UpdateAsync(Professional entity)
        {
            _context.Professionals.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Professionals.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return;

            entity.Enabled = false;
            _context.Professionals.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task ReactivateAsync(int id)
        {
            var entity = await _context.Professionals.FirstOrDefaultAsync(p => p.Id == id);
            if (entity == null) return;

            entity.Enabled = true;
            _context.Professionals.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
