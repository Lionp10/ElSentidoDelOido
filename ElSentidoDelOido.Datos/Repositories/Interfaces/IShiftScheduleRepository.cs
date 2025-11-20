using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IShiftScheduleRepository
    {
        Task<IEnumerable<ShiftSchedule>> GetByShiftTypeAsync(int shiftTypeId);
        Task<IEnumerable<Shift>> GetShiftsByTypeAndDateAsync(int shiftTypeId, DateTime date);
    }
}
