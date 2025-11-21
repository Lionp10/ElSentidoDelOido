using System;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class HolidaysDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Message { get; set; }
    }

    public class HolidaysCreateDTO
    {
        public DateTime Date { get; set; }
        public string? Message { get; set; }
    }

    public class HolidaysUpdateDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Message { get; set; }
    }
}
