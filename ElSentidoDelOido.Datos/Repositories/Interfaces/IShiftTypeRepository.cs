using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IShiftTypeRepository
    {
        Task<IEnumerable<ShiftType>> GetAllAsync();
        Task<ShiftType?> GetByIdAsync(int id);

        Task<ShiftType> CreateAsync(ShiftType entity);
        Task<ShiftType> UpdateAsync(ShiftType entity);
        Task DeleteAsync(int id); 
        Task ReactivateAsync(int id); 
    }
}
