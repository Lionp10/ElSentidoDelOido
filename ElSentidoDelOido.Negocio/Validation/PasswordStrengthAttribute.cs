using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ElSentidoDelOido.Negocio.Validation
{
    /// <summary>
    /// Validación de fuerza de contraseña (servidor).
    /// No implementa interfaces de MVC para evitar referencias a Microsoft.AspNetCore en el proyecto 'Negocio'.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PasswordStrengthAttribute : ValidationAttribute
    {
        public int MinLength { get; set; } = 8;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialCharacter { get; set; } = true;

        public PasswordStrengthAttribute()
        {
            ErrorMessage = "La contraseña no cumple los requisitos de seguridad.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password))
            {
                // Dejar que [Required] maneje vacío si también está aplicado.
                return ValidationResult.Success;
            }

            var errors = ValidatePassword(password);
            if (errors == null) return ValidationResult.Success;

            return new ValidationResult(errors, new[] { validationContext.MemberName ?? string.Empty });
        }

        private string? ValidatePassword(string password)
        {
            if (password.Length < MinLength)
                return string.Format(CultureInfo.InvariantCulture, "La contraseña debe tener al menos {0} caracteres.", MinLength);

            if (RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
                return "La contraseña debe contener al menos una letra mayúscula.";

            if (RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
                return "La contraseña debe contener al menos una letra minúscula.";

            if (RequireDigit && !Regex.IsMatch(password, "[0-9]"))
                return "La contraseña debe contener al menos un número.";

            if (RequireSpecialCharacter && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                return "La contraseña debe contener al menos un carácter especial (p. ej. !@#$%).";

            if (password.Contains(" "))
                return "La contraseña no puede contener espacios.";

            return null;
        }
    }
}