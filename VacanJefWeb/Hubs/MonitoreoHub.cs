using Microsoft.AspNetCore.SignalR;

namespace VacanJefWeb.Hubs;

// Simula el canal en tiempo real del CU-03 (nivel de actividad, temperatura,
// hora de última comida) mientras no haya cámaras/sensores físicos integrados.
// Para conectar cámaras reales: reemplazar GenerarMetricaSimulada() por los
// datos que entreguen tus sensores/RTSP, manteniendo el mismo contrato de grupo.
public class MonitoreoHub : Hub
{
    public async Task UnirseAReserva(int idReserva)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GrupoDeReserva(idReserva));
    }

    public async Task SalirDeReserva(int idReserva)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoDeReserva(idReserva));
    }

    public static string GrupoDeReserva(int idReserva) => $"reserva-{idReserva}";
}
