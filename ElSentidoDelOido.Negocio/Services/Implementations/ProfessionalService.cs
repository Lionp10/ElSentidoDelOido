using AutoMapper;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class ProfessionalService : IProfessionalService
    {
        private readonly IProfessionalRepository _repository;
        private readonly IMapper _mapper;

        public ProfessionalService(IProfessionalRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProfessionalDTO>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProfessionalDTO>>(items);
        }

        public async Task<ProfessionalDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<ProfessionalDTO>(entity);
        }

        public async Task<ProfessionalDTO> CreateAsync(ProfessionalCreateDTO dto)
        {
            var entity = _mapper.Map<Professional>(dto);

            if (!entity.Enabled.HasValue) entity.Enabled = dto.Enabled;

            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProfessionalDTO>(created);
        }

        public async Task<ProfessionalDTO> UpdateAsync(ProfessionalUpdateDTO dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Professional with id {dto.Id} not found.");

            _mapper.Map(dto, existing);

            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<ProfessionalDTO>(updated);
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
