using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDTO>> GetActiveAsync();
        Task<RoleDTO?> GetByIdAsync(int id);
    }
}
