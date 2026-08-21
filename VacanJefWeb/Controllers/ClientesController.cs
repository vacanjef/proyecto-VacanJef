using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize(Roles = RolUsuario.Administrador)]
public class ClientesController : Controller
{
    private readonly VacanJefContext _db;
    public ClientesController(VacanJefContext db) => _db = db;

    // ── Listado con filtros de fecha y búsqueda ────────────────────
    public async Task<IActionResult> Index(
        string? filtro, DateTime? desde, DateTime? hasta, string? q)
    {
        filtro ??= "todos";
        var hoy     = DateTime.Today;
        var iniFsem = hoy.AddDays(-(int)hoy.DayOfWeek + 1);
        var iniFmes = new DateTime(hoy.Year, hoy.Month, 1);

        var query = _db.Clientes.AsQueryable();

        // Filtro por fecha de registro
        query = filtro switch
        {
            "hoy"    => query.Where(c => c.FechaRegistro.Date == hoy),
            "semana" => query.Where(c => c.FechaRegistro.Date >= iniFsem && c.FechaRegistro.Date <= hoy),
            "mes"    => query.Where(c => c.FechaRegistro.Date >= iniFmes && c.FechaRegistro.Date <= hoy),
            "rango"  => query.Where(c =>
                            (!desde.HasValue || c.FechaRegistro.Date >= desde.Value.Date) &&
                            (!hasta.HasValue  || c.FechaRegistro.Date <= hasta.Value.Date)),
            _        => query,
        };

        // Búsqueda por nombre/apellido/correo
        if (!string.IsNullOrWhiteSpace(q))
        {
            var busq = q.Trim().ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(busq)  ||
                c.Apellido.ToLower().Contains(busq) ||
                (c.Correo != null && c.Correo.ToLower().Contains(busq)));
        }

        var todos = await _db.Clientes.CountAsync();
        var modelo = new ClienteListaViewModel
        {
            Clientes    = await query.OrderByDescending(c => c.FechaRegistro).ToListAsync(),
            Busqueda    = q,
            Filtro      = filtro,
            FechaDesde  = desde,
            FechaHasta  = hasta,
            TotalHoy    = await _db.Clientes.CountAsync(c => c.FechaRegistro.Date == hoy),
            TotalSemana = await _db.Clientes.CountAsync(c => c.FechaRegistro.Date >= iniFsem),
            TotalMes    = await _db.Clientes.CountAsync(c => c.FechaRegistro.Date >= iniFmes),
            TotalGeneral = todos,
        };

        return View(modelo);
    }

    // ── Detalle: info del cliente + mascotas + reservas ────────────
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _db.Clientes
            .Include(c => c.Mascotas)
            .Include(c => c.Reservas)
                .ThenInclude(r => r.Mascota)
            .Include(c => c.Reservas)
                .ThenInclude(r => r.TipoServicio)
            .Include(c => c.Contactos)
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        if (cliente is null) return NotFound();

        ViewBag.TotalReservas  = cliente.Reservas.Count;
        ViewBag.TotalMascotas  = cliente.Mascotas.Count(m => m.Activo);
        ViewBag.ReservasActivas = cliente.Reservas
            .Count(r => r.EstadoReserva == EstadoReserva.Confirmada
                     || r.EstadoReserva == EstadoReserva.EnCurso);

        return View(cliente);
    }
}
