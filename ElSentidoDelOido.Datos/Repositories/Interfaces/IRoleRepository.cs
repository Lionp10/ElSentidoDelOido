using ElSentidoDelOido.Datos.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Datos.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<UserRole>> GetActiveAsync();
        Task<UserRole?> GetByIdAsync(int id);
    }
}
