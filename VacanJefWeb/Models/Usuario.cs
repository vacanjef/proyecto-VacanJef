namespace VacanJefWeb.Models;

public static class RolUsuario
{
    public const string Administrador = "Administrador";
    public const string Empleado = "Empleado";
    public const string Cliente = "Cliente";
}

// Tabla nueva, separada de Clientes/Empleados a propósito: el esquema original
// del proyecto no traía credenciales, así que la autenticación vive aquí y se
// enlaza por id_cliente / id_empleado sin tocar las tablas que ya existían.
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = RolUsuario.Cliente;
    public int? IdCliente { get; set; }
    public int? IdEmpleado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public Cliente? Cliente { get; set; }
    public Empleado? Empleado { get; set; }
}
