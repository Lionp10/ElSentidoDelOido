using System;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class ContactMessageDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Read { get; set; }
        public bool Answered { get; set; }
        public string? Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
    }

    public class ContactMessageCreateDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
