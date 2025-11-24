using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IContactMessageService
    {
        Task<ContactMessageDTO> CreateAsync(ContactMessageCreateDTO dto);
        Task<IEnumerable<ContactMessageDTO>> GetAllAsync();
        Task<ContactMessageDTO?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
        Task ReplyAsync(int id, string reply);
        Task DeleteAsync(int id);
        Task<int> GetUnreadCountAsync();
    }
}
