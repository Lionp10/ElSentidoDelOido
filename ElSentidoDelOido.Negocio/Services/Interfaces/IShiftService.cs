using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftDTO>> GetAllAsync();
        Task<ShiftDTO?> GetByIdAsync(int id);
        Task<ShiftDTO> CreateAsync(ShiftCreateDTO dto);
        Task<ShiftDTO> UpdateAsync(ShiftDTO dto);
        Task DeleteAsync(int id);        
        Task<(IEnumerable<ShiftDTO> Items, int TotalCount)> GetPagedAsync(
            DateTime? fecha, 
            string? estado, 
            int? tipoTurnoId, 
            int? professionalId, 
            int page, 
            int pageSize);
        Task<ShiftDTO> ApproveAsync(int shiftId, int professionalId);
        Task<ShiftDTO> RejectAsync(int shiftId);
        Task<ShiftDTO> CancelAsync(int shiftId);
        Task<ShiftDTO> CompleteAsync(int shiftId);
    }
}
