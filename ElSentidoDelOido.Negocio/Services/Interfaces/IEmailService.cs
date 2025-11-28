namespace ElSentidoDelOido.Negocio.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? fromEmail = null, string? fromName = null, string? smtpProfile = null);
    }
}
