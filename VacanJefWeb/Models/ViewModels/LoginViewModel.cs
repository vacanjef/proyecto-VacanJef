using System.ComponentModel.DataAnnotations;

namespace VacanJefWeb.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresa tu correo")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu contraseña")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
