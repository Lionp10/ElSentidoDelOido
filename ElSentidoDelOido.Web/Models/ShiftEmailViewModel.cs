namespace ElSentidoDelOido.Web.Models
{
    public class ShiftEmailViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Hour { get; set; } = string.Empty;
        public string ShiftType { get; set; } = string.Empty;
        public string Professional { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public string PatientNote { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string? LogoDataUri { get; set; }
    }
}
