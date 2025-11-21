using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public UserRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Cargar Role si existe para que quede disponible en el resultado
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return user;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException($"User with id {id} not found.");

            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalized = email.Trim().ToLowerInvariant();

            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalized);
        }
    }
}
