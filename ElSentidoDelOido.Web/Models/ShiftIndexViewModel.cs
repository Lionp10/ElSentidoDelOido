using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Web.Models
{
    public class ShiftIndexViewModel
    {
        public IEnumerable<ShiftTypeDTO> TiposTurnos { get; set; } = new List<ShiftTypeDTO>();
    }
}
