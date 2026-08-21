using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize]
public class MonitoreoController : Controller
{
    private readonly VacanJefContext _db;
    public MonitoreoController(VacanJefContext db) => _db = db;

    // Hub de cámaras activas — Admin y Empleado ven todas;
    // Cliente ve solo las suyas con token de acceso directo.
    public async Task<IActionResult> Index()
    {
        var query = _db.Reservas
            .Include(r => r.Mascota)
            .Include(r => r.Cliente)
            .Include(r => r.Empleado)
            .Where(r => r.EstadoReserva == EstadoReserva.Confirmada
                     || r.EstadoReserva == EstadoReserva.EnCurso)
            .AsQueryable();

        if (User.IsInRole(RolUsuario.Cliente))
        {
            var idCliente = int.TryParse(User.FindFirstValue("id_cliente"), out var c) ? c : 0;
            query = query.Where(r => r.IdCliente == idCliente);
        }

        var reservas = await query.OrderByDescending(r => r.FechaInicio).ToListAsync();
        var ids      = reservas.Select(r => r.IdReserva).ToList();

        // Traer los tokens activos de estas reservas
        var accesos = await _db.AccesosCamara
            .Where(a => ids.Contains(a.IdReserva) && a.Activo && a.FechaExpiracion > DateTime.Now)
            .ToDictionaryAsync(a => a.IdReserva, a => a.Token);

        ViewBag.Accesos = accesos;
        return View(reservas);
    }
}
