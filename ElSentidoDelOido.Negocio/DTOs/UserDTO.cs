using System;
using System.ComponentModel.DataAnnotations;

namespace ElSentidoDelOido.Negocio.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public int? RoleId { get; set; }

        // Propiedad opcional para mostrar el nombre del rol si lo mapeas desde la entidad
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
        public string? Password { get; set; }

        [Required(ErrorMessage = "Debes repetir la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmPassword { get; set; }

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
        public string? Password { get; set; } // null = no cambiar

        public int? RoleId { get; set; }

        public bool? Enabled { get; set; }
    }
}
