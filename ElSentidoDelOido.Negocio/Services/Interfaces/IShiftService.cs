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
        
        // Método para paginación con filtros
        Task<(IEnumerable<ShiftDTO> Items, int TotalCount)> GetPagedAsync(
            DateTime? fecha, 
            string? estado, 
            int? tipoTurnoId, 
            int? professionalId, 
            int page, 
            int pageSize);

        // Nuevos métodos para aprobar y rechazar
        Task<ShiftDTO> ApproveAsync(int shiftId, int professionalId);
        Task<ShiftDTO> RejectAsync(int shiftId);

        // Agregar estos métodos al interface
        Task<ShiftDTO> CancelAsync(int shiftId);
        Task<ShiftDTO> CompleteAsync(int shiftId);
    }
}
