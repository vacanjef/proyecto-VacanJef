namespace VacanJefWeb.Models;

public class Cargo
{
    public int IdCargo { get; set; }
    public string NombreCargo { get; set; } = string.Empty;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
