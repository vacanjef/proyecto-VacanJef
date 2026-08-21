using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly VacanJefContext _db;
    public DashboardController(VacanJefContext db) => _db = db;

    [Authorize]
    public IActionResult Index()
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        return RedirectToDashboard(rol);
    }

    // ── ADMIN ─────────────────────────────────────────────────────
    // ── Página de reportes y gráficas (separada del welcome panel) ──
    [Authorize(Roles = RolUsuario.Administrador)]
    public async Task<IActionResult> Reportes()
    {
        var reportes = await ConstruirReportes();

        var hace6Meses = DateTime.Now.AddMonths(-5);

        var reservasPorMes = await _db.Reservas
            .Where(r => r.FechaCreacion >= hace6Meses)
            .GroupBy(r => new { r.FechaCreacion.Year, r.FechaCreacion.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        var facturasPorMes = await _db.Facturas
            .Where(f => f.FechaEmision >= hace6Meses && f.EstadoPago == Models.EstadoPago.Pagado)
            .GroupBy(f => new { f.FechaEmision.Year, f.FechaEmision.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(f => f.Total) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        var meses = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddMonths(-5 + i)).ToList();

        var ci = System.Globalization.CultureInfo.GetCultureInfo("es-CO");
        ViewBag.Meses    = System.Text.Json.JsonSerializer.Serialize(
            meses.Select(m => ci.DateTimeFormat.GetAbbreviatedMonthName(m.Month).ToUpper()).ToList());
        ViewBag.Reservas = System.Text.Json.JsonSerializer.Serialize(
            meses.Select(m => reservasPorMes.FirstOrDefault(r => r.Year==m.Year && r.Month==m.Month)?.Total ?? 0).ToList());
        ViewBag.Ingresos = System.Text.Json.JsonSerializer.Serialize(
            meses.Select(m => facturasPorMes.FirstOrDefault(f => f.Year==m.Year && f.Month==m.Month)?.Total ?? 0).ToList());
        ViewBag.Estados  = System.Text.Json.JsonSerializer.Serialize(new[] {
            reportes.ReservasPendientes, reportes.ReservasConfirmadas,
            reportes.ReservasEnCurso, reportes.ReservasFinalizadas, reportes.ReservasCanceladas });
        ViewBag.Reportes = reportes;

        return View(reportes);
    }

    [Authorize(Roles = RolUsuario.Administrador)]
    public async Task<IActionResult> Admin()
    {
        var (nombre, cargo) = await ObtenerNombreEmpleado();
        ViewBag.Nombre  = nombre;
        ViewBag.Cargo   = string.IsNullOrEmpty(cargo) ? "Administrador" : cargo;
        ViewBag.Reportes = await ConstruirReportes();

        // ── Datos para gráficas ──────────────────────────────────
        // Reservas por mes (últimos 6 meses)
        var hace6Meses = DateTime.Now.AddMonths(-5);
        var reservasPorMes = await _db.Reservas
            .Where(r => r.FechaCreacion >= hace6Meses)
            .GroupBy(r => new { r.FechaCreacion.Year, r.FechaCreacion.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        // Ingresos por mes (últimos 6 meses)
        var facturasPorMes = await _db.Facturas
            .Where(f => f.FechaEmision >= hace6Meses && f.EstadoPago == Models.EstadoPago.Pagado)
            .GroupBy(f => new { f.FechaEmision.Year, f.FechaEmision.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(f => f.Total) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        // Generar etiquetas de los últimos 6 meses
        var meses = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddMonths(-5 + i))
            .ToList();

        var mesesNombres = meses.Select(m =>
            System.Globalization.CultureInfo.GetCultureInfo("es-CO")
                .DateTimeFormat.GetAbbreviatedMonthName(m.Month).ToUpper()).ToList();

        var reservasMes = meses.Select(m =>
            reservasPorMes.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Total ?? 0).ToList();

        var ingresosMes = meses.Select(m =>
            facturasPorMes.FirstOrDefault(f => f.Year == m.Year && f.Month == m.Month)?.Total ?? 0).ToList();

        // Estados de reservas para dona
        var reportes = (ReportesViewModel)ViewBag.Reportes;
        ViewBag.GraficaMeses       = System.Text.Json.JsonSerializer.Serialize(mesesNombres);
        ViewBag.GraficaReservas    = System.Text.Json.JsonSerializer.Serialize(reservasMes);
        ViewBag.GraficaIngresos    = System.Text.Json.JsonSerializer.Serialize(ingresosMes);
        ViewBag.GraficaEstados     = System.Text.Json.JsonSerializer.Serialize(new[] {
            reportes.ReservasPendientes, reportes.ReservasConfirmadas,
            reportes.ReservasEnCurso, reportes.ReservasFinalizadas, reportes.ReservasCanceladas });

        return View();
    }

    // ── EMPLEADO ──────────────────────────────────────────────────
    [Authorize(Roles = RolUsuario.Empleado)]
    public async Task<IActionResult> Empleado()
    {
        var (nombre, cargo) = await ObtenerNombreEmpleado();
        ViewBag.Nombre = nombre;
        ViewBag.Cargo  = string.IsNullOrEmpty(cargo) ? "Empleado" : cargo;

        var idEmpleado = int.TryParse(User.FindFirstValue("id_empleado"), out var e) ? e : 0;
        ViewBag.ReservasHoy = await _db.Reservas
            .Include(r => r.Mascota)
            .Where(r => r.IdEmpleado == idEmpleado && r.FechaInicio.Date == DateTime.Today)
            .ToListAsync();
        return View();
    }

    // ── CLIENTE ───────────────────────────────────────────────────
    [Authorize(Roles = RolUsuario.Cliente)]
    public async Task<IActionResult> Cliente()
    {
        var idCliente = int.TryParse(User.FindFirstValue("id_cliente"), out var c) ? c : 0;
        var cli = await _db.Clientes.FindAsync(idCliente);
        ViewBag.Nombre = cli?.Nombre ?? "Cliente";

        ViewBag.Mascotas = await _db.Mascotas
            .Where(m => m.IdCliente == idCliente && m.Activo).ToListAsync();

        var proximasReservas = await _db.Reservas
            .Include(r => r.Mascota).Include(r => r.TipoServicio)
            .Where(r => r.IdCliente == idCliente && r.EstadoReserva != EstadoReserva.Cancelada)
            .OrderByDescending(r => r.FechaInicio).Take(5).ToListAsync();
        ViewBag.ProximasReservas = proximasReservas;

        // Buscar acceso a cámaras activo (reserva confirmada o en curso)
        var reservaActiva = proximasReservas
            .FirstOrDefault(r => r.EstadoReserva == EstadoReserva.Confirmada
                              || r.EstadoReserva == EstadoReserva.EnCurso);
        if (reservaActiva is not null)
        {
            var acceso = await _db.AccesosCamara
                .FirstOrDefaultAsync(a => a.IdReserva == reservaActiva.IdReserva && a.Activo);
            if (acceso is not null)
            {
                ViewBag.CameraToken    = acceso.Token;
                ViewBag.CameraMascota  = reservaActiva.Mascota?.Nombre;
            }
        }
        return View();
    }

    // ── Helpers ───────────────────────────────────────────────────
    private async Task<(string nombre, string cargo)> ObtenerNombreEmpleado()
    {
        if (!int.TryParse(User.FindFirstValue("id_empleado"), out int idEmpleado))
            return ("Admin", "");
        var emp = await _db.Empleados.Include(e => e.Cargo)
            .FirstOrDefaultAsync(e => e.IdEmpleado == idEmpleado);
        return (emp?.Nombre ?? "Admin", emp?.Cargo?.NombreCargo ?? "");
    }

    private IActionResult RedirectToDashboard(string? rol) => rol switch
    {
        RolUsuario.Administrador => RedirectToAction("Admin"),
        RolUsuario.Empleado      => RedirectToAction("Empleado"),
        _                        => RedirectToAction("Cliente"),
    };

    private async Task<ReportesViewModel> ConstruirReportes()
    {
        var reservas = await _db.Reservas.ToListAsync();
        var facturas = await _db.Facturas.ToListAsync();
        return new ReportesViewModel
        {
            ReservasPendientes   = reservas.Count(r => r.EstadoReserva == EstadoReserva.Pendiente),
            ReservasConfirmadas  = reservas.Count(r => r.EstadoReserva == EstadoReserva.Confirmada),
            ReservasEnCurso      = reservas.Count(r => r.EstadoReserva == EstadoReserva.EnCurso),
            ReservasFinalizadas  = reservas.Count(r => r.EstadoReserva == EstadoReserva.Finalizada),
            ReservasCanceladas   = reservas.Count(r => r.EstadoReserva == EstadoReserva.Cancelada),
            IngresosPagados      = facturas.Where(f => f.EstadoPago == Models.EstadoPago.Pagado).Sum(f => f.Total),
            IngresosPendientes   = facturas.Where(f => f.EstadoPago != Models.EstadoPago.Pagado).Sum(f => f.Total),
            TotalMascotasActivas = await _db.Mascotas.CountAsync(m => m.Activo),
            TotalClientesActivos = await _db.Clientes.CountAsync(c => c.Activo),
        };
    }
}
