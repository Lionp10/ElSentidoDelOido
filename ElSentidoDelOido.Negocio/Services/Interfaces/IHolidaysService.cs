using System;
using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IHolidaysService
    {
        Task<IEnumerable<HolidaysDTO>> GetAllAsync();
        Task<HolidaysDTO?> GetByIdAsync(int id);
        Task<HolidaysDTO> CreateAsync(HolidaysCreateDTO dto);
        Task<HolidaysDTO> UpdateAsync(HolidaysUpdateDTO dto);
        Task DeleteAsync(int id);
    }
}
