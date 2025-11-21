using ElSentidoDelOido.Datos.Entities;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<User?> GetByEmailAsync(string email);

    }
}
