namespace VacanJefWeb.Models;

// Cada vez que una reserva pasa a "Confirmada", el sistema genera
// un token único que permite al dueño ver las cámaras SIN necesitar
// una cuenta. El enlace funciona como una llave temporal.
public class AccesoCamara
{
    public int    IdAcceso         { get; set; }
    public int    IdReserva        { get; set; }
    public string Token            { get; set; } = string.Empty;
    public DateTime FechaCreacion  { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool   Activo           { get; set; } = true;

    public Reserva? Reserva { get; set; }
}
