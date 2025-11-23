using AutoMapper;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Entities.Enums;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _repository;
        private readonly IMapper _mapper;

        public ShiftService(IShiftRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShiftDTO>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShiftDTO>>(entities);
        }

        public async Task<ShiftDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<ShiftDTO>(entity);
        }

        public async Task<ShiftDTO> CreateAsync(ShiftCreateDTO dto)
        {
            // Validaciones
            if (dto.Date.Date < DateTime.Today)
            {
                throw new InvalidOperationException("No se pueden crear turnos para fechas pasadas.");
            }

            if (string.IsNullOrWhiteSpace(dto.Hour))
            {
                throw new ArgumentException("La hora es obligatoria.", nameof(dto.Hour));
            }

            // Buscar el schedule correspondiente
            var schedule = await _repository.GetScheduleByHourAndTypeAsync(dto.Hour.Trim(), dto.TipoTurnoId);
            if (schedule == null)
            {
                throw new InvalidOperationException("El horario seleccionado no existe para este tipo de turno.");
            }

            // Crear la entidad Shift
            var entity = new Shift
            {
                FirstName = dto.FirstName?.Trim(),
                LastName = dto.LastName?.Trim(),
                Email = dto.Email?.Trim().ToLowerInvariant(),
                Phone = dto.Phone?.Trim(),
                Date = dto.Date.Date,
                ScheduleId = schedule.Id,
                Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
                ShiftTypeId = dto.TipoTurnoId,
                ShiftStateId = ShiftStateEnum.Pendiente.ToString(), // Por defecto "Pendiente"
                ProfessionalId = null // Se asignará posteriormente por el administrador
            };

            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<ShiftDTO>(created);
        }

        public async Task<ShiftDTO> UpdateAsync(ShiftDTO dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Turno con id {dto.Id} no encontrado.");
            }

            _mapper.Map(dto, existing);
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<ShiftDTO>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        // Nuevo método para paginación con filtros
        public async Task<(IEnumerable<ShiftDTO> Items, int TotalCount)> GetPagedAsync(
            DateTime? fecha,
            string? estado,
            int? tipoTurnoId,
            int? professionalId,
            int page,
            int pageSize)
        {
            var (items, totalCount) = await _repository.GetPagedAsync(fecha, estado, tipoTurnoId, professionalId, page, pageSize);
            var dtos = _mapper.Map<IEnumerable<ShiftDTO>>(items);
            return (dtos, totalCount);
        }

        // Nuevo método para aprobar turno
        public async Task<ShiftDTO> ApproveAsync(int shiftId, int professionalId)
        {
            var shift = await _repository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con id {shiftId} no encontrado.");
            }

            if (shift.ShiftStateId != ShiftStateEnum.Pendiente.ToString())
            {
                throw new InvalidOperationException("Solo se pueden aprobar turnos que estén pendientes.");
            }

            shift.ShiftStateId = ShiftStateEnum.Confirmado.ToString();
            shift.ProfessionalId = professionalId;

            var updated = await _repository.UpdateAsync(shift);
            return _mapper.Map<ShiftDTO>(updated);
        }

        // Nuevo método para rechazar turno
        public async Task<ShiftDTO> RejectAsync(int shiftId)
        {
            var shift = await _repository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con id {shiftId} no encontrado.");
            }

            if (shift.ShiftStateId != ShiftStateEnum.Pendiente.ToString())
            {
                throw new InvalidOperationException("Solo se pueden rechazar turnos que estén pendientes.");
            }

            shift.ShiftStateId = ShiftStateEnum.Rechazado.ToString();

            var updated = await _repository.UpdateAsync(shift);
            return _mapper.Map<ShiftDTO>(updated);
        }

        /// <summary>
        /// Cancelar un turno confirmado
        /// </summary>
        public async Task<ShiftDTO> CancelAsync(int shiftId)
        {
            var shift = await _repository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con id {shiftId} no encontrado.");
            }

            if (shift.ShiftStateId != ShiftStateEnum.Confirmado.ToString())
            {
                throw new InvalidOperationException("Solo se pueden cancelar turnos que estén confirmados.");
            }

            shift.ShiftStateId = ShiftStateEnum.Cancelado.ToString();

            var updated = await _repository.UpdateAsync(shift);
            return _mapper.Map<ShiftDTO>(updated);
        }

        /// <summary>
        /// Marcar un turno como culminado
        /// </summary>
        public async Task<ShiftDTO> CompleteAsync(int shiftId)
        {
            var shift = await _repository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Turno con id {shiftId} no encontrado.");
            }

            if (shift.ShiftStateId != ShiftStateEnum.Confirmado.ToString())
            {
                throw new InvalidOperationException("Solo se pueden culminar turnos que estén confirmados.");
            }

            // Opcional: Validar que la fecha del turno ya haya pasado
            if (shift.Date.HasValue && shift.Date.Value.Date > DateTime.Today)
            {
                throw new InvalidOperationException("No se puede culminar un turno futuro.");
            }

            shift.ShiftStateId = ShiftStateEnum.Culminado.ToString();

            var updated = await _repository.UpdateAsync(shift);
            return _mapper.Map<ShiftDTO>(updated);
        }
    }
}
