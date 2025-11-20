using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IShiftTypeService
    {
        Task<IEnumerable<ShiftTypeDTO>> GetAllAsync();
        Task<ShiftTypeDTO?> GetByIdAsync(int id);
    }
}
