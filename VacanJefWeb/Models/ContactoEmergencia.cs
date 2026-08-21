namespace VacanJefWeb.Models;

public class ContactoEmergencia
{
    public int IdContacto { get; set; }
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Parentesco { get; set; }

    public Cliente? Cliente { get; set; }
}
