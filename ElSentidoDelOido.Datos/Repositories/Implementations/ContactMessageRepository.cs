using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly ElSentidoDelOidoDBContext _db;

        public ContactMessageRepository(ElSentidoDelOidoDBContext db)
        {
            _db = db;
        }

        public async Task<ContactMessage> AddAsync(ContactMessage entity)
        {
            await _db.ContactMessages.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<ContactMessage>> GetAllAsync()
        {
            return await _db.ContactMessages
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _db.ContactMessages.FindAsync(id);
        }

        public async Task UpdateAsync(ContactMessage entity)
        {
            _db.ContactMessages.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.ContactMessages.FindAsync(id);
            if (existing == null) return;
            _db.ContactMessages.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _db.ContactMessages.CountAsync(x => !x.Read);
        }
    }
}
