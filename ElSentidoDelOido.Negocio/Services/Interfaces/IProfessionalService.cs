using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IProfessionalService
    {
        Task<IEnumerable<ProfessionalDTO>> GetAllAsync();
        Task<ProfessionalDTO?> GetByIdAsync(int id);
        Task<ProfessionalDTO> CreateAsync(ProfessionalCreateDTO dto);
        Task<ProfessionalDTO> UpdateAsync(ProfessionalUpdateDTO dto);
        Task DeleteAsync(int id);
        Task ReactivateAsync(int id);
    }
}
