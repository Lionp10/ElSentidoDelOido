using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IShiftScheduleService
    {
        Task<IEnumerable<ShiftScheduleDTO>> GetAvailabilityAsync(int shiftTypeId, DateTime date);
    }
}
