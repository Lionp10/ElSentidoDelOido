using AutoMapper;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class HolidaysService : IHolidaysService
    {
        private readonly IHolidaysRepository _repository;
        private readonly IMapper _mapper;

        public HolidaysService(IHolidaysRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HolidaysDTO>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<HolidaysDTO>>(entities);
        }

        public async Task<HolidaysDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<HolidaysDTO>(entity);
        }

        public async Task<HolidaysDTO> CreateAsync(HolidaysCreateDTO dto)
        {
            if (dto.Date == default) throw new ArgumentException("Date is required", nameof(dto.Date));

            if (await _repository.ExistsByDateAsync(dto.Date))
                throw new InvalidOperationException("Ya existe un feriado para esa fecha.");

            var entity = _mapper.Map<Datos.Entities.Holidays>(dto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<HolidaysDTO>(created);
        }

        public async Task<HolidaysDTO> UpdateAsync(HolidaysUpdateDTO dto)
        {
            if (dto.Date == default) throw new ArgumentException("Date is required", nameof(dto.Date));

            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null) throw new KeyNotFoundException($"Holidays with id {dto.Id} not found.");

            if (await _repository.ExistsByDateAsync(dto.Date, dto.Id))
                throw new InvalidOperationException("Ya existe un feriado para esa fecha.");

            _mapper.Map(dto, existing);
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<HolidaysDTO>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
