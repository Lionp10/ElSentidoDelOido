using System.Collections.Generic;
using System.Threading.Tasks;
using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);
        Task<UserDTO> CreateAsync(UserCreateDTO dto);
        Task<UserDTO> UpdateAsync(UserUpdateDTO dto);
        Task DeleteAsync(int id);
    }
}
