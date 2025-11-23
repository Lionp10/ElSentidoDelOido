using ElSentidoDelOido.Datos.DataContext;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElSentidoDelOido.Datos.Repositories.Implementations
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly ElSentidoDelOidoDBContext _context;

        public ShiftRepository(ElSentidoDelOidoDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Shift>> GetAllAsync()
        {
            return await _context.Shifts
                .Include(s => s.ShiftType)
                .Include(s => s.Schedule)
                .Include(s => s.Professional)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Shift?> GetByIdAsync(int id)
        {
            return await _context.Shifts
                .Include(s => s.ShiftType)
                .Include(s => s.Schedule)
                .Include(s => s.Professional)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Shift> CreateAsync(Shift entity)
        {
            _context.Shifts.Add(entity);
            await _context.SaveChangesAsync();
            
            // Recargar con las entidades relacionadas
            return await GetByIdAsync(entity.Id) ?? entity;
        }

        public async Task<Shift> UpdateAsync(Shift entity)
        {
            _context.Shifts.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Shifts.FindAsync(id);
            if (entity == null) return;
            
            _context.Shifts.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ShiftSchedule?> GetScheduleByHourAndTypeAsync(string hour, int shiftTypeId)
        {
            return await _context.ShiftSchedules
                .FirstOrDefaultAsync(s => s.Hour == hour && s.ShiftTypeId == shiftTypeId);
        }

        // Nuevo método para paginación con filtros
        public async Task<(IEnumerable<Shift> Items, int TotalCount)> GetPagedAsync(
            DateTime? fecha, 
            string? estado, 
            int? tipoTurnoId, 
            int? professionalId, 
            int page, 
            int pageSize)
        {
            var query = _context.Shifts
                .Include(s => s.ShiftType)
                .Include(s => s.Schedule)
                .Include(s => s.Professional)
                .AsQueryable();

            // Aplicar filtros
            if (fecha.HasValue)
            {
                var fechaFiltro = fecha.Value.Date;
                query = query.Where(s => s.Date.HasValue && s.Date.Value.Date == fechaFiltro);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(s => s.ShiftStateId == estado);
            }

            if (tipoTurnoId.HasValue && tipoTurnoId.Value > 0)
            {
                query = query.Where(s => s.ShiftTypeId == tipoTurnoId.Value);
            }

            if (professionalId.HasValue && professionalId.Value > 0)
            {
                query = query.Where(s => s.ProfessionalId == professionalId.Value);
            }

            // Contar total
            var totalCount = await query.CountAsync();

            // Aplicar paginación y ordenar por fecha descendente
            var items = await query
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
