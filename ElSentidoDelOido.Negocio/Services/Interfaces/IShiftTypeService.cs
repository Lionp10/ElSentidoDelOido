using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IShiftTypeService
    {
        Task<IEnumerable<ShiftTypeDTO>> GetAllAsync();
        Task<ShiftTypeDTO?> GetByIdAsync(int id);
        Task<ShiftTypeDTO> CreateAsync(ShiftTypeCreateDTO dto);
        Task<ShiftTypeDTO> UpdateAsync(ShiftTypeUpdateDTO dto);
        Task DeleteAsync(int id);
        Task ReactivateAsync(int id);
    }
}
