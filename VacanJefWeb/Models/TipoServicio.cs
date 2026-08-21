namespace VacanJefWeb.Models;

public class TipoServicio
{
    public int IdTipoServicio { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioBase { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
