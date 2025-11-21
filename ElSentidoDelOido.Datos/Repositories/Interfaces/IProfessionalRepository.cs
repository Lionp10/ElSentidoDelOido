using ElSentidoDelOido.Datos.Entities;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IProfessionalRepository
    {
        Task<IEnumerable<Professional>> GetAllAsync();
        Task<Professional?> GetByIdAsync(int id);
        Task<Professional> CreateAsync(Professional entity);
        Task<Professional> UpdateAsync(Professional entity);
        Task DeleteAsync(int id); // soft delete: Enabled = false
        Task ReactivateAsync(int id); // opcional para reactivar
    }
}
