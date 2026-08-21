namespace VacanJefWeb.Models.ViewModels;

public class MascotaMonitoreoViewModel
{
    public int IdReserva { get; set; }
    public int IdMascota { get; set; }
    public string NombreMascota { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string EstadoReserva { get; set; } = string.Empty;
    public string? NombreCuidador { get; set; }
}

public class ReportesViewModel
{
    public int ReservasPendientes { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int ReservasEnCurso { get; set; }
    public int ReservasFinalizadas { get; set; }
    public int ReservasCanceladas { get; set; }

    public decimal IngresosPagados { get; set; }
    public decimal IngresosPendientes { get; set; }

    public int TotalMascotasActivas { get; set; }
    public int TotalClientesActivos { get; set; }
}
