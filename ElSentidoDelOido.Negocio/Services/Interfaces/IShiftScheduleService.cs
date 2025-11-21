using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IShiftScheduleService
    {
        Task<IEnumerable<ShiftScheduleDTO>> GetAvailabilityAsync(int shiftTypeId, DateTime date);

        // nuevo: listado paginado y CRUD
        Task<(IEnumerable<ShiftScheduleDTO> Items, int TotalCount)> GetPagedAsync(int? shiftTypeId, int page, int pageSize);
        Task<ShiftScheduleDTO?> GetByIdAsync(int id);
        Task<ShiftScheduleDTO> CreateAsync(ShiftScheduleCreateDTO dto);
        Task<ShiftScheduleDTO> UpdateAsync(ShiftScheduleUpdateDTO dto);
        Task DeleteAsync(int id);
    }
}
