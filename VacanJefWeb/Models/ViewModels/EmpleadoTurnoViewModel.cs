namespace VacanJefWeb.Models.ViewModels;

public class EmpleadoTurnoViewModel
{
    public int     IdEmpleado    { get; set; }
    public string  Nombre        { get; set; } = string.Empty;
    public string  Apellido      { get; set; } = string.Empty;
    public string? Correo        { get; set; }
    public string? Telefono      { get; set; }
    public string  Cargo         { get; set; } = string.Empty;
    public TimeSpan? TurnoInicio { get; set; }
    public TimeSpan? TurnoFin    { get; set; }
    public List<string> DiasLaborales { get; set; } = new();
    public bool EnTurnoAhora { get; set; }
    public bool TrabajaHoy   { get; set; }
}
