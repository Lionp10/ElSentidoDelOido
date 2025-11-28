using ElSentidoDelOido.Negocio.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ElSentidoDelOido.Negocio.Services.Implementations
{
    public class GoogleRecaptchaService : IGoogleRecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleRecaptchaService> _logger;
        private readonly HttpClient _httpClient;

        public GoogleRecaptchaService(IConfiguration configuration, ILogger<GoogleRecaptchaService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token de reCAPTCHA vacío o nulo");
                return false;
            }

            try
            {
                var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
                var minimumScore = double.Parse(_configuration["GoogleReCaptcha:MinimumScore"] ?? "0.5");

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey ?? string.Empty),
                    new KeyValuePair<string, string>("response", token)
                });

                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GoogleRecaptchaResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("Respuesta de reCAPTCHA nula");
                    return false;
                }

                if (!result.Success)
                {
                    _logger.LogWarning("Verificación de reCAPTCHA falló: {Errors}", string.Join(", ", result.ErrorCodes ?? new string[] { }));
                    return false;
                }

                if (result.Score < minimumScore)
                {
                    _logger.LogWarning("Score de reCAPTCHA muy bajo: {Score} (mínimo: {MinScore})", result.Score, minimumScore);
                    return false;
                }

                _logger.LogInformation("reCAPTCHA verificado exitosamente. Score: {Score}, Action: {Action}", result.Score, result.Action);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar token de reCAPTCHA");
                return false;
            }
        }

        private class GoogleRecaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string? Action { get; set; }
            public DateTime ChallengeTs { get; set; }
            public string? Hostname { get; set; }
            public string[]? ErrorCodes { get; set; }
        }
    }
}
