using AutoMapper;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class ShiftTypeService : IShiftTypeService
    {
        private readonly IShiftTypeRepository _repository;
        private readonly IMapper _mapper;

        public ShiftTypeService(IShiftTypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShiftTypeDTO>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShiftTypeDTO>>(entities);
        }

        public async Task<ShiftTypeDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<ShiftTypeDTO>(entity);
        }

        public async Task<ShiftTypeDTO> CreateAsync(ShiftTypeCreateDTO dto)
        {
            var entity = _mapper.Map<Datos.Entities.ShiftType>(dto);
            // si quieres alguna regla adicional la agregas aquí
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<ShiftTypeDTO>(created);
        }

        public async Task<ShiftTypeDTO> UpdateAsync(ShiftTypeUpdateDTO dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"ShiftType with id {dto.Id} not found.");

            // aplicar cambios
            _mapper.Map(dto, existing);
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<ShiftTypeDTO>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task ReactivateAsync(int id)
        {
            await _repository.ReactivateAsync(id);
        }
    }
}
