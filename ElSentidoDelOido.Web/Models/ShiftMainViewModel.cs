using ElSentidoDelOido.Negocio.DTOs;

namespace ElSentidoDelOido.Web.Models
{
    public class ShiftMainViewModel
    {
        public IEnumerable<ShiftDTO> Items { get; set; } = new List<ShiftDTO>();
        
        // Filtros
        public DateTime? FechaFiltro { get; set; }
        public string? EstadoFiltro { get; set; }
        public int? TipoTurnoFiltro { get; set; }
        public int? ProfessionalFiltro { get; set; }

        // Para llenar los dropdowns de filtros
        public IEnumerable<ShiftTypeDTO> ShiftTypes { get; set; } = new List<ShiftTypeDTO>();
        public IEnumerable<ProfessionalDTO> Professionals { get; set; } = new List<ProfessionalDTO>();

        // Estados disponibles del enum
        public IDictionary<string, string> EstadosDisponibles { get; set; } = new Dictionary<string, string>();

        // Paginación
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}
