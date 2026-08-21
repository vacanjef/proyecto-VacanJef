using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Hubs;
using VacanJefWeb.Models;

namespace VacanJefWeb.Services;

public class MonitoreoSimuladoService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<MonitoreoHub> _hub;
    private readonly ILogger<MonitoreoSimuladoService> _logger;
    private readonly Random _rng = new();

    public MonitoreoSimuladoService(IServiceScopeFactory scopeFactory, IHubContext<MonitoreoHub> hub, ILogger<MonitoreoSimuladoService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<VacanJefContext>();

                var idsActivos = await db.Reservas
                    .Where(r => r.EstadoReserva == EstadoReserva.Confirmada || r.EstadoReserva == EstadoReserva.EnCurso)
                    .Select(r => r.IdReserva)
                    .ToListAsync(stoppingToken);

                foreach (var idReserva in idsActivos)
                {
                    var metrica = new
                    {
                        idReserva,
                        nivelActividad = _rng.Next(10, 100),
                        temperaturaC = Math.Round(20 + _rng.NextDouble() * 6, 1),
                        ultimaComida = DateTime.Now.AddHours(-_rng.Next(0, 6)).ToString("HH:mm"),
                        timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    };

                    await _hub.Clients.Group(MonitoreoHub.GrupoDeReserva(idReserva))
                        .SendAsync("metricaActualizada", metrica, cancellationToken: stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // la app se está apagando, nada que hacer
                break;
            }
            catch (Exception ex)
            {
                // IMPORTANTE: si un BackgroundService deja escapar una excepción sin
                // capturar, ASP.NET Core apaga TODA la aplicación, no solo este
                // servicio. Por eso cualquier error (timeout de SQL, conexión caída,
                // etc.) se atrapa aquí, se registra en el log, y el ciclo continúa
                // en el siguiente intento 4 segundos después, sin tumbar el sitio.
                _logger.LogWarning(ex, "No se pudo actualizar el monitoreo simulado en este ciclo. Se reintentará en 4 segundos.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(4), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
