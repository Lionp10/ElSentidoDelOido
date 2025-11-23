using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IShiftScheduleRepository
    {
        Task<IEnumerable<ShiftSchedule>> GetByShiftTypeAsync(int shiftTypeId);
        Task<IEnumerable<Shift>> GetShiftsByTypeAndDateAsync(int shiftTypeId, DateTime date);      
        Task<IEnumerable<Shift>> GetShiftsByTypeAndDateWithBlockingStatesAsync(int shiftTypeId, DateTime date);
        Task<IEnumerable<ShiftSchedule>> GetAllAsync();
        Task<ShiftSchedule?> GetByIdAsync(int id);
        Task<ShiftSchedule> CreateAsync(ShiftSchedule entity);
        Task<ShiftSchedule> UpdateAsync(ShiftSchedule entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsByHourAndTypeAsync(string hour, int shiftTypeId, int? excludeId = null);
    }
}
