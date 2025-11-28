using AutoMapper;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class ShiftScheduleService : IShiftScheduleService
    {
        private readonly IShiftScheduleRepository _repository;
        private readonly IMapper _mapper;

        public ShiftScheduleService(IShiftScheduleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShiftScheduleDTO>> GetAvailabilityAsync(int shiftTypeId, DateTime date)
        {
            var schedules = (await _repository.GetByShiftTypeAsync(shiftTypeId)).ToList();
            
            var occupiedShifts = (await _repository.GetShiftsByTypeAndDateWithBlockingStatesAsync(shiftTypeId, date))
                                    .Where(s => s.ScheduleId.HasValue)
                                    .Select(s => s.ScheduleId!.Value)
                                    .ToHashSet();

            var isToday = date.Date == DateTime.Today;
            var nowTime = DateTime.Now.TimeOfDay;

            var dtos = schedules
                .Select(s =>
                {
                    var available = !occupiedShifts.Contains(s.Id);

                    if (available && isToday)
                    {
                        if (!string.IsNullOrWhiteSpace(s.Hour))
                        {
                            if (TimeSpan.TryParse(s.Hour.Trim(), out var scheduleTime))
                            {
                                if (scheduleTime <= nowTime)
                                {
                                    available = false;
                                }
                            }
                        }
                    }

                    return new ShiftScheduleDTO
                    {
                        Id = s.Id,
                        Hour = s.Hour,
                        Enabled = available,
                        ShiftTypeId = s.ShiftTypeId,
                        ShiftTypeName = s.ShiftType?.Name
                    };
                })
                .OrderBy(s => s.Hour)
                .ToList();

            return dtos;
        }

        public async Task<(IEnumerable<ShiftScheduleDTO> Items, int TotalCount)> GetPagedAsync(int? shiftTypeId, int page, int pageSize)
        {
            IEnumerable<ShiftSchedule> source;
            if (shiftTypeId.HasValue && shiftTypeId.Value > 0)
            {
                source = await _repository.GetByShiftTypeAsync(shiftTypeId.Value);
            }
            else
            {
                source = await _repository.GetAllAsync();
            }

            var ordered = source.OrderBy(s => s.Hour).ToList();
            var total = ordered.Count;
            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShiftScheduleDTO
                {
                    Id = s.Id,
                    Hour = s.Hour,
                    Enabled = true,
                    ShiftTypeId = s.ShiftTypeId,
                    ShiftTypeName = s.ShiftType?.Name
                })
                .ToList();

            return (items, total);
        }

        public async Task<ShiftScheduleDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return new ShiftScheduleDTO
            {
                Id = entity.Id,
                Hour = entity.Hour,
                Enabled = true,
                ShiftTypeId = entity.ShiftTypeId,
                ShiftTypeName = entity.ShiftType?.Name
            };
        }

        public async Task<ShiftScheduleDTO> CreateAsync(ShiftScheduleCreateDTO dto)
        {
            if (!dto.ShiftTypeId.HasValue)
                throw new ArgumentException("ShiftTypeId is required", nameof(dto.ShiftTypeId));

            var exists = await _repository.ExistsByHourAndTypeAsync(dto.Hour!.Trim(), dto.ShiftTypeId.Value);
            if (exists)
                throw new InvalidOperationException("Ya existe un horario igual para el tipo de turno seleccionado.");

            var entity = new ShiftSchedule
            {
                Hour = dto.Hour!.Trim(),
                ShiftTypeId = dto.ShiftTypeId
            };

            var created = await _repository.CreateAsync(entity);

            var reloaded = await _repository.GetByIdAsync(created.Id);

            return new ShiftScheduleDTO
            {
                Id = created.Id,
                Hour = created.Hour,
                Enabled = true,
                ShiftTypeId = reloaded?.ShiftTypeId,
                ShiftTypeName = reloaded?.ShiftType?.Name
            };
        }

        public async Task<ShiftScheduleDTO> UpdateAsync(ShiftScheduleUpdateDTO dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Horario con id {dto.Id} no encontrado.");

            if (!dto.ShiftTypeId.HasValue)
                throw new ArgumentException("ShiftTypeId is required", nameof(dto.ShiftTypeId));

            var hourNormalized = dto.Hour!.Trim();
            var duplicate = await _repository.ExistsByHourAndTypeAsync(hourNormalized, dto.ShiftTypeId.Value, excludeId: dto.Id);
            if (duplicate)
                throw new InvalidOperationException("Ya existe un horario igual para el tipo de turno seleccionado.");

            existing.Hour = hourNormalized;
            existing.ShiftTypeId = dto.ShiftTypeId;

            var updated = await _repository.UpdateAsync(existing);

            var reloaded = await _repository.GetByIdAsync(updated.Id);

            return new ShiftScheduleDTO
            {
                Id = updated.Id,
                Hour = updated.Hour,
                Enabled = true,
                ShiftTypeId = reloaded?.ShiftTypeId,
                ShiftTypeName = reloaded?.ShiftType?.Name
            };
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
