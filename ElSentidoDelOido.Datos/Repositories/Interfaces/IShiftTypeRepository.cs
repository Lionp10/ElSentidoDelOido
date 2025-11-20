using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IShiftTypeRepository
    {
        Task<IEnumerable<ShiftType>> GetAllAsync();
        Task<ShiftType?> GetByIdAsync(int id);
    }
}
