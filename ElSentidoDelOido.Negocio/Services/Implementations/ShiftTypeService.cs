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
    }
}
