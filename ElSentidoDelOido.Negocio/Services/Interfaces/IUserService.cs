using System.Threading.Tasks;
using ElSentidoDelOido.Negocio.DTOs;
using System.Collections.Generic;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);
        Task<UserDTO> CreateAsync(UserCreateDTO dto);
        Task<UserDTO> UpdateAsync(UserUpdateDTO dto);
        Task DeleteAsync(int id);
        Task<UserDTO?> AuthenticateAsync(string email, string password);
    }
}
