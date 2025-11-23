using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IShiftRepository
    {
        Task<IEnumerable<Shift>> GetAllAsync();
        Task<Shift?> GetByIdAsync(int id);
        Task<Shift> CreateAsync(Shift entity);
        Task<Shift> UpdateAsync(Shift entity);
        Task DeleteAsync(int id);
        
        // Método específico para buscar un schedule por hora y tipo de turno
        Task<ShiftSchedule?> GetScheduleByHourAndTypeAsync(string hour, int shiftTypeId);
        
        // Nuevo método para paginación con filtros
        Task<(IEnumerable<Shift> Items, int TotalCount)> GetPagedAsync(
            DateTime? fecha, 
            string? estado, 
            int? tipoTurnoId, 
            int? professionalId, 
            int page, 
            int pageSize);
    }
}
