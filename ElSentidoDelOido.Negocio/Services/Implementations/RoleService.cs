using ElSentidoDelOido.Datos.Repositories.Interfaces;
using ElSentidoDelOido.Negocio.DTOs;
using ElSentidoDelOido.Negocio.Services.Interfaces;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDTO>> GetActiveAsync()
        {
            var roles = await _roleRepository.GetActiveAsync();
            return roles.Select(r => new RoleDTO
            {
                Id = r.Id,
                Name = r.Name,
                Enabled = r.Enabled
            });
        }

        public async Task<RoleDTO?> GetByIdAsync(int id)
        {
            var r = await _roleRepository.GetByIdAsync(id);
            if (r == null) return null;
            return new RoleDTO { Id = r.Id, Name = r.Name, Enabled = r.Enabled };
        }
    }
}
