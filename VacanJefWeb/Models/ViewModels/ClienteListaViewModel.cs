namespace VacanJefWeb.Models.ViewModels;

public class ClienteListaViewModel
{
    public List<Models.Cliente> Clientes  { get; set; } = new();
    public string?   Busqueda   { get; set; }
    public string    Filtro     { get; set; } = "todos";
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int TotalHoy     { get; set; }
    public int TotalSemana  { get; set; }
    public int TotalMes     { get; set; }
    public int TotalGeneral { get; set; }
}
