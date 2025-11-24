using System;
using System.ComponentModel.DataAnnotations;
using ElSentidoDelOido.Negocio.Validation;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public int? RoleId { get; set; }

        public string? RoleName { get; set; }

        public DateTime DateOfCreation { get; set; }

        public bool? Enabled { get; set; }
    }

    public class UserCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [PasswordStrength(MinLength = 8, RequireUppercase = true, RequireLowercase = true, RequireDigit = true, RequireSpecialCharacter = true,
            ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, incluir mayúscula, minúscula, número y carácter especial.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Debes repetir la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Rol")]
        public int? RoleId { get; set; }

        [Display(Name = "Activo")]
        public bool Enabled { get; set; } = true;
    }

    public class UserUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public int? RoleId { get; set; }

        public bool Enabled { get; set; } = true;
    }
}
