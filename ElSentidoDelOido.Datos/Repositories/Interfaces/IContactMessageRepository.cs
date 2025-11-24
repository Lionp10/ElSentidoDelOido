using ElSentidoDelOido.Datos.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IContactMessageRepository
    {
        Task<ContactMessage> AddAsync(ContactMessage entity);
        Task<IEnumerable<ContactMessage>> GetAllAsync();
        Task<ContactMessage?> GetByIdAsync(int id);
        Task UpdateAsync(ContactMessage entity);
        Task DeleteAsync(int id);
        Task<int> GetUnreadCountAsync();
    }
}
