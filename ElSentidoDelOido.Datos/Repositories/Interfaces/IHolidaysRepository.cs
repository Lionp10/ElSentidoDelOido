using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IHolidaysRepository
    {
        Task<IEnumerable<Holidays>> GetAllAsync();
        Task<Holidays?> GetByIdAsync(int id);
        Task<Holidays> CreateAsync(Holidays entity);
        Task<Holidays> UpdateAsync(Holidays entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsByDateAsync(DateTime date, int? excludeId = null);
    }
}
