using ElSentidoDelOido.Negocio.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ElSentidoDelOido.Web.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? fromEmail = null, string? fromName = null, string? smtpProfile = null)
        {
            IConfigurationSection section;

            if (!string.IsNullOrWhiteSpace(smtpProfile))
            {
                section = _configuration.GetSection($"SmtpProfiles:{smtpProfile}");
                // si el perfil no está bien definido, fallback a Smtp
                if (string.IsNullOrWhiteSpace(section["Host"]))
                {
                    _logger.LogWarning("Perfil SMTP '{Profile}' no encontrado. Usando sección 'Smtp' por defecto.", smtpProfile);
                    section = _configuration.GetSection("Smtp");
                }
            }
            else
            {
                section = _configuration.GetSection("Smtp");
            }

            var host = section["Host"] ?? throw new InvalidOperationException("Smtp:Host no configurado");
            var port = 25;
            if (!string.IsNullOrWhiteSpace(section["Port"]) && int.TryParse(section["Port"], out var p)) port = p;

            var user = section["User"] ?? string.Empty;
            var pass = section["Pass"] ?? string.Empty;
            var defaultFrom = section["FromEmail"] ?? user;
            var defaultFromName = section["FromName"] ?? "El Sentido del Oído";

            var useSsl = true;
            if (!string.IsNullOrWhiteSpace(section["UseSsl"]) && bool.TryParse(section["UseSsl"], out var ssl)) useSsl = ssl;

            var timeoutSeconds = 60;
            if (!string.IsNullOrWhiteSpace(section["TimeoutSeconds"]) && int.TryParse(section["TimeoutSeconds"], out var t)) timeoutSeconds = t;

            var skipCertValidation = false;
            if (!string.IsNullOrWhiteSpace(section["SkipCertificateValidation"]) && bool.TryParse(section["SkipCertificateValidation"], out var sc)) skipCertValidation = sc;

            var message = new MimeMessage();
            var from = new MailboxAddress(fromName ?? defaultFromName, fromEmail ?? defaultFrom);
            message.From.Add(from);
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            var socketOptionsCandidates = new List<SecureSocketOptions>();

            if (port == 465)
                socketOptionsCandidates.Add(SecureSocketOptions.SslOnConnect);
            if (useSsl)
            {
                socketOptionsCandidates.Add(SecureSocketOptions.StartTls);
                socketOptionsCandidates.Add(SecureSocketOptions.StartTlsWhenAvailable);
            }
            socketOptionsCandidates.Add(SecureSocketOptions.Auto);
            socketOptionsCandidates.Add(SecureSocketOptions.None);

            socketOptionsCandidates = socketOptionsCandidates.Distinct().ToList();

            Exception? lastEx = null;

            foreach (var socketOptions in socketOptionsCandidates)
            {
                _logger.LogInformation("Intentando enviar email a {To} usando host={Host}, port={Port}, socket={Socket}, profile={Profile}", toEmail, host, port, socketOptions, smtpProfile ?? "default");
                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.Timeout = timeoutSeconds * 1000;

                if (skipCertValidation)
                {
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                    {
                        _logger.LogWarning("Omitiendo validación de certificado TLS para {Host}:{Port}", host, port);
                        return true;
                    };
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                try
                {
                    await client.ConnectAsync(host, port, socketOptions, cts.Token);

                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(user, pass, cts.Token);
                    }

                    await client.SendAsync(message, cts.Token);
                    await client.DisconnectAsync(true, cts.Token);

                    _logger.LogInformation("Email enviado a {To} con socket {Socket} (profile={Profile})", toEmail, socketOptions, smtpProfile ?? "default");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al intentar enviar email a {To} con socket {Socket} (host={Host},port={Port})", toEmail, socketOptions, host, port);
                    lastEx = ex;
                }
                finally
                {
                    try { if (client.IsConnected) await client.DisconnectAsync(true); } catch { }
                }

                await Task.Delay(500);
            }

            _logger.LogError("No se pudo enviar el correo a {To} tras {Attempts} intentos. Host={Host} Port={Port} Profile={Profile}", toEmail, socketOptionsCandidates.Count, host, port, smtpProfile ?? "default");
            throw lastEx ?? new InvalidOperationException("No se pudo enviar el correo (intentos agotados).");
        }
    }
}