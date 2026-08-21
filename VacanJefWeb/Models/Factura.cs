namespace VacanJefWeb.Models;

public static class EstadoPago
{
    public const string Pagado = "Pagado";
    public const string Pendiente = "Pendiente";
    public const string SinCancelar = "Sin cancelar";
    public const string Anulado = "Anulado";
}

public class Factura
{
    public int IdFactura { get; set; }
    public int IdCliente { get; set; }
    public int IdReserva { get; set; }
    public int? IdMetodoPago { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal ImpuestoPct { get; set; }

    // Columna calculada en SQL Server (PERSISTED): subtotal - descuento + impuesto.
    // Solo lectura desde EF Core, nunca se escribe.
    public decimal Total { get; private set; }

    public string EstadoPago { get; set; } = Models.EstadoPago.Pendiente;
    public string? Notas { get; set; }

    public Cliente? Cliente { get; set; }
    public Reserva? Reserva { get; set; }
    public MetodoPago? MetodoPago { get; set; }
}
