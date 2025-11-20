using AutoMapper;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

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
            var occupiedShifts = (await _repository.GetShiftsByTypeAndDateAsync(shiftTypeId, date))
                                    .Where(s => s.ScheduleId.HasValue)
                                    .Select(s => s.ScheduleId!.Value)
                                    .ToHashSet();

            var dtos = schedules
                .Select(s => new ShiftScheduleDTO
                {
                    Id = s.Id,
                    Hora = s.Hour,
                    Disponible = !occupiedShifts.Contains(s.Id)
                })
                .OrderBy(s => s.Hora)  // orden por hora si el formato lo permite
                .ToList();

            return dtos;
        }
    }
}
