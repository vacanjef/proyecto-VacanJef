namespace VacanJefWeb.Models;

public class Mascota
{
    public int IdMascota { get; set; }
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public string? Sexo { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Color { get; set; }
    public decimal? PesoKg { get; set; }
    public string? Alergias { get; set; }
    public string? Medicamentos { get; set; }
    public string? Observaciones { get; set; }
    public string? CarnetVacunas { get; set; }
    public bool Activo { get; set; } = true;

    public Cliente? Cliente { get; set; }
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
