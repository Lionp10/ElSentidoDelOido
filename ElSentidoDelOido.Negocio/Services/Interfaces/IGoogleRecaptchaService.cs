using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IGoogleRecaptchaService
    {
        Task<bool> VerifyTokenAsync(string token);
    }
}
