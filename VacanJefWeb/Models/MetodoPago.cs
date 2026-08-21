namespace VacanJefWeb.Models;

public class MetodoPago
{
    public int IdMetodoPago { get; set; }
    public string NombreMetodo { get; set; } = string.Empty;

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
