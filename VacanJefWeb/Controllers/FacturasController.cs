using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;

namespace VacanJefWeb.Controllers;

[Authorize]
public class FacturasController : Controller
{
    private readonly VacanJefContext _db;
    public FacturasController(VacanJefContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var query = _db.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Reserva).ThenInclude(r => r!.Mascota)
            .Include(f => f.Reserva).ThenInclude(r => r!.TipoServicio)
            .Include(f => f.MetodoPago)
            .AsQueryable();

        // ── Seguridad reforzada ──────────────────────────────────
        // Si el rol es Cliente, SIEMPRE se filtra por su id_cliente.
        // Si por algún motivo el claim no viene en la cookie (sesión
        // antigua, token corrupto, etc.) se cierra sesión y se pide
        // volver a entrar, en vez de arriesgarse a mostrar datos de
        // otro cliente con un valor por defecto silencioso.
        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");

            query = query.Where(f => f.IdCliente == idCliente);
        }

        ViewBag.MetodosPago = await _db.MetodosPago.ToListAsync();
        return View(await query.OrderByDescending(f => f.FechaEmision).ToListAsync());
    }

    // ── Detalle / recibo individual de una factura ───────────────
    // Cualquier rol puede entrar, pero un Cliente SOLO puede ver
    // su propia factura — se verifica el id_cliente del dueño real
    // contra el de la sesión antes de mostrar nada.
    public async Task<IActionResult> Details(int id)
    {
        var factura = await _db.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.MetodoPago)
            .Include(f => f.Reserva).ThenInclude(r => r!.Mascota)
            .Include(f => f.Reserva).ThenInclude(r => r!.TipoServicio)
            .Include(f => f.Reserva).ThenInclude(r => r!.Empleado)
            .FirstOrDefaultAsync(f => f.IdFactura == id);

        if (factura is null) return NotFound();

        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");

            // Verificación de propiedad: esta factura tiene que ser de ESTE cliente.
            if (factura.IdCliente != idCliente)
                return Forbid();
        }

        return View(factura);
    }

    // Paso 2a del CU-04: el administrador registra pagos (también en efectivo).
    [Authorize(Roles = $"{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPagada(int id, int idMetodoPago)
    {
        var factura = await _db.Facturas.FindAsync(id);
        if (factura is null) return NotFound();

        factura.EstadoPago = EstadoPago.Pagado;
        factura.IdMetodoPago = idMetodoPago;
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Factura #{id} marcada como pagada.";
        return RedirectToAction(nameof(Index));
    }

    // Genera la factura de una reserva confirmada/finalizada que aún no la tiene (CU-04, paso 5).
    [Authorize(Roles = $"{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(int idReserva)
    {
        var yaExiste = await _db.Facturas.AnyAsync(f => f.IdReserva == idReserva);
        if (yaExiste)
        {
            TempData["Mensaje"] = "Esa reserva ya tiene una factura generada.";
            return RedirectToAction(nameof(Index));
        }

        var reserva = await _db.Reservas.Include(r => r.TipoServicio).FirstOrDefaultAsync(r => r.IdReserva == idReserva);
        if (reserva is null) return NotFound();

        // El precio_base de "Estadia" es una tarifa diaria; "Paseo" es por visita.
        // Si hay fecha_fin, se cobran los días que dure la estadía (mínimo 1).
        var precioBase = reserva.TipoServicio?.PrecioBase ?? 0;
        var dias = reserva.FechaFin is not null
            ? Math.Max(1, (int)Math.Ceiling((reserva.FechaFin.Value - reserva.FechaInicio).TotalDays))
            : 1;
        var esEstadia = (reserva.TipoServicio?.NombreServicio ?? "").Contains("Estadia", StringComparison.OrdinalIgnoreCase);
        var subtotal = esEstadia ? precioBase * dias : precioBase;

        var factura = new Factura
        {
            IdCliente = reserva.IdCliente,
            IdReserva = reserva.IdReserva,
            FechaEmision = DateTime.Now,
            Subtotal = subtotal,
            Descuento = 0,
            ImpuestoPct = 0,
            EstadoPago = EstadoPago.Pendiente,
        };

        _db.Facturas.Add(factura);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = "Factura generada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
