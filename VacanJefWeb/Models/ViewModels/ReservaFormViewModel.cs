using System.ComponentModel.DataAnnotations;

namespace VacanJefWeb.Models.ViewModels;

public class ReservaFormViewModel
{
    // Solo aplica cuando un Admin/Empleado crea la reserva en nombre de un cliente
    public int? IdClienteSeleccionado { get; set; }

    [Required(ErrorMessage = "Selecciona una mascota")]
    public int IdMascota { get; set; }

    [Required(ErrorMessage = "Selecciona un tipo de servicio")]
    public int IdTipoServicio { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    [DataType(DataType.DateTime)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? FechaFin { get; set; }

    public string? Observaciones { get; set; }

    // Para repoblar el formulario
    public List<MascotaOpcion> MascotasDisponibles { get; set; } = new();
    public List<TipoServicioOpcion> ServiciosDisponibles { get; set; } = new();
    public List<ClienteOpcion> ClientesDisponibles { get; set; } = new();
    public decimal? CostoEstimado { get; set; }
}

public class ClienteOpcion
{
    public int IdCliente { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}

public class MascotaOpcion
{
    public int IdMascota { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class TipoServicioOpcion
{
    public int IdTipoServicio { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
}
