using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Web.Models
{
    public class ShiftMainViewModel
    {
        public IEnumerable<ShiftDTO> Items { get; set; } = new List<ShiftDTO>();
        
        public DateTime? FechaFiltro { get; set; }
        public string? EstadoFiltro { get; set; }
        public int? TipoTurnoFiltro { get; set; }
        public int? ProfessionalFiltro { get; set; }

        public IEnumerable<ShiftTypeDTO> ShiftTypes { get; set; } = new List<ShiftTypeDTO>();
        public IEnumerable<ProfessionalDTO> Professionals { get; set; } = new List<ProfessionalDTO>();

        public IDictionary<string, string> EstadosDisponibles { get; set; } = new Dictionary<string, string>();

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
