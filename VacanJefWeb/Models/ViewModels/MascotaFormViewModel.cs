using System.ComponentModel.DataAnnotations;

namespace VacanJefWeb.Models.ViewModels;

public class MascotaFormViewModel
{
    public int IdMascota { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie es obligatoria")]
    public string Especie { get; set; } = string.Empty;

    public string? Raza { get; set; }

    [RegularExpression("^[MH]$", ErrorMessage = "Usa M o H")]
    public string? Sexo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    public string? Color { get; set; }

    [Range(0, 200, ErrorMessage = "Peso fuera de rango")]
    public decimal? PesoKg { get; set; }

    public string? Alergias { get; set; }
    public string? Medicamentos { get; set; }
    public string? Observaciones { get; set; }
    public string? CarnetVacunas { get; set; }

    // Solo lo usa el administrador/empleado al registrar la mascota de un cliente (CU-01, flujo alterno 6a)
    public int? IdClienteSeleccionado { get; set; }
}
