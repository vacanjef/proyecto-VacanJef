namespace VacanJefWeb.Models;

public static class EstadoReserva
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string EnCurso = "En curso";
    public const string Finalizada = "Finalizada";
    public const string Cancelada = "Cancelada";
}

public class Reserva
{
    public int IdReserva { get; set; }
    public int IdCliente { get; set; }
    public int IdMascota { get; set; }
    public int? IdEmpleado { get; set; }
    public int? IdTipoServicio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string EstadoReserva { get; set; } = Models.EstadoReserva.Pendiente;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Cliente? Cliente { get; set; }
    public Mascota? Mascota { get; set; }
    public Empleado? Empleado { get; set; }
    public TipoServicio? TipoServicio { get; set; }
    public Factura? Factura { get; set; }
}
