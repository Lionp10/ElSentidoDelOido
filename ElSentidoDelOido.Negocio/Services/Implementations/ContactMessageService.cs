using AutoMapper;
using ElSentidoDelOido.Datos.Entities;
using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IContactMessageRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactMessageService> _logger;

        public ContactMessageService(IContactMessageRepository repo, IMapper mapper, ILogger<ContactMessageService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ContactMessageDTO> CreateAsync(ContactMessageCreateDTO dto)
        {
            var entity = _mapper.Map<ContactMessage>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var created = await _repo.AddAsync(entity);
            return _mapper.Map<ContactMessageDTO>(created);
        }

        public async Task<IEnumerable<ContactMessageDTO>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ContactMessageDTO>>(list);
        }

        public async Task<ContactMessageDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<ContactMessageDTO>(entity);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Mensaje no encontrado");
            if (!entity.Read)
            {
                entity.Read = true;
                await _repo.UpdateAsync(entity);
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            var list = await _repo.GetAllAsync();
            var toUpdate = list.Where(x => !x.Read).ToList();
            if (!toUpdate.Any()) return;

            foreach (var item in toUpdate)
            {
                item.Read = true;
                await _repo.UpdateAsync(item);
            }

            _logger.LogInformation("Marcados {Count} mensajes como leídos.", toUpdate.Count);
        }

        public async Task ReplyAsync(int id, string reply)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Mensaje no encontrado");

            entity.Reply = reply;
            entity.Answered = true;
            entity.RepliedAt = DateTime.UtcNow;
            entity.Read = true;

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _repo.GetUnreadCountAsync();
        }
    }
}
