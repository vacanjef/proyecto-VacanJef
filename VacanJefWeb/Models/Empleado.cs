namespace VacanJefWeb.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Correo { get; set; }
    public int? IdCargo { get; set; }
    public decimal? Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public bool Activo { get; set; } = true;
    public string? EmergenciaNombre { get; set; }
    public string? EmergenciaTelefono { get; set; }
    public string? EmergenciaParentesco { get; set; }

    // ── Turno laboral ──────────────────────────────────────
    // DiasLaborales: lista corta separada por coma, ej "Lun,Mar,Mie,Jue,Vie"
    public TimeSpan? TurnoInicio { get; set; }
    public TimeSpan? TurnoFin { get; set; }
    public string? DiasLaborales { get; set; }

    public Cargo? Cargo { get; set; }
    public ICollection<Reserva> ReservasAsignadas { get; set; } = new List<Reserva>();
}
