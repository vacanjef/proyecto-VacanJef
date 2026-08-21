using System.ComponentModel.DataAnnotations;

namespace VacanJefWeb.Models.ViewModels;

// ── Registro de nuevo CLIENTE ─────────────────────────────────────────────────
// Crea simultáneamente: una fila en Clientes + una fila en Usuarios (con bcrypt)
// + opcionalmente un ContactoEmergencia. Campos 1:1 con el esquema SQL.
public class RegistroClienteViewModel
{
    // ── Datos personales (tabla Clientes) ─────────────────────────
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Número no válido")]
    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    // ── Credenciales (tabla Usuarios) ─────────────────────────────
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    // ── Contacto de emergencia opcional (tabla Contactos_Emergencia) ──
    [StringLength(100)]
    [Display(Name = "Nombre del contacto de emergencia")]
    public string? ContactoNombre { get; set; }

    [StringLength(100)]
    [Display(Name = "Apellido del contacto")]
    public string? ContactoApellido { get; set; }

    [Phone]
    [StringLength(20)]
    [Display(Name = "Teléfono de emergencia")]
    public string? ContactoTelefono { get; set; }

    [StringLength(50)]
    [Display(Name = "Parentesco")]
    public string? ContactoParentesco { get; set; }
}

// ── Registro de EMPLEADO (solo Admin puede hacerlo) ───────────────────────────
// Crea: una fila en Empleados + una fila en Usuarios.
public class RegistroEmpleadoViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña temporal")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Cargo")]
    public int IdCargo { get; set; }

    [Range(0, 99999999)]
    [Display(Name = "Salario mensual ($)")]
    public decimal? Salario { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de ingreso")]
    public DateTime FechaIngreso { get; set; } = DateTime.Today;

    // ── Contacto de emergencia del empleado ──
    [StringLength(150)]
    [Display(Name = "Contacto de emergencia")]
    public string? EmergenciaNombre { get; set; }

    [Phone]
    [StringLength(20)]
    [Display(Name = "Teléfono de emergencia")]
    public string? EmergenciaTelefono { get; set; }

    [StringLength(50)]
    [Display(Name = "Parentesco")]
    public string? EmergenciaParentesco { get; set; }

    // Para repoblar el <select> de cargos
    public List<CargoOpcion> CargosDisponibles { get; set; } = new();
}

public class CargoOpcion
{
    public int IdCargo { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
}
