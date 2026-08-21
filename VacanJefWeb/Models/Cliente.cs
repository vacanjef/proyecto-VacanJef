namespace VacanJefWeb.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Correo { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public ICollection<ContactoEmergencia> Contactos { get; set; } = new List<ContactoEmergencia>();
}
