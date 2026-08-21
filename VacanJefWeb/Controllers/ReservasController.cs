using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize]
public class ReservasController : Controller
{
    private readonly VacanJefContext _db;
    public ReservasController(VacanJefContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var query = _db.Reservas
            .Include(r => r.Mascota)
            .Include(r => r.Cliente)
            .Include(r => r.TipoServicio)
            .Include(r => r.Empleado)
            .AsQueryable();

        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");
            query = query.Where(r => r.IdCliente == idCliente);
        }

        var reservas = await query.OrderByDescending(r => r.FechaInicio).ToListAsync();

        // Cargar tokens de cámara para mostrar botón "Ver" en cada fila
        var ids = reservas.Select(r => r.IdReserva).ToList();
        var accesos = await _db.AccesosCamara
            .Where(a => ids.Contains(a.IdReserva) && a.Activo)
            .Select(a => new { a.IdReserva, a.Token })
            .ToDictionaryAsync(a => a.IdReserva, a => a.Token);
        ViewBag.AccesosCamara = accesos;

        return View(reservas);
    }

    // ── CU-02: el dueño crea la reserva (o un Admin/Empleado en su nombre) ──
    [Authorize(Roles = $"{RolUsuario.Cliente},{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");
            return View(await ArmarFormulario(idCliente));
        }

        // Admin/Empleado: arrancan sin cliente seleccionado, eligen en el formulario
        return View(await ArmarFormulario(null));
    }

    [Authorize(Roles = $"{RolUsuario.Cliente},{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservaFormViewModel modelo)
    {
        int idCliente;

        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");
        }
        else
        {
            // Admin/Empleado deben elegir explícitamente el cliente dueño de la mascota
            if (modelo.IdClienteSeleccionado is null)
            {
                ModelState.AddModelError(nameof(modelo.IdClienteSeleccionado), "Selecciona el cliente.");
                var rehechoSinCliente = await ArmarFormulario(null);
                rehechoSinCliente.IdTipoServicio = modelo.IdTipoServicio;
                rehechoSinCliente.FechaInicio = modelo.FechaInicio;
                rehechoSinCliente.FechaFin = modelo.FechaFin;
                rehechoSinCliente.Observaciones = modelo.Observaciones;
                return View(rehechoSinCliente);
            }
            idCliente = modelo.IdClienteSeleccionado.Value;
        }

        // La mascota debe ser efectivamente del cliente indicado (dueño real, sin importar quién la registró).
        var mascotaValida = await _db.Mascotas.AnyAsync(m => m.IdMascota == modelo.IdMascota && m.IdCliente == idCliente);
        if (!mascotaValida) ModelState.AddModelError(nameof(modelo.IdMascota), "Mascota no válida para este cliente.");

        if (modelo.FechaFin is not null && modelo.FechaFin <= modelo.FechaInicio)
            ModelState.AddModelError(nameof(modelo.FechaFin), "La fecha de salida debe ser posterior a la de ingreso.");

        // ── Validar disponibilidad: que la mascota no tenga otra reserva activa que se cruce en fechas ──
        var finComparable = modelo.FechaFin ?? modelo.FechaInicio.AddHours(1);
        var cruce = await _db.Reservas.AnyAsync(r =>
            r.IdMascota == modelo.IdMascota &&
            r.EstadoReserva != EstadoReserva.Cancelada &&
            r.EstadoReserva != EstadoReserva.Finalizada &&
            r.FechaInicio < finComparable &&
            (r.FechaFin ?? r.FechaInicio.AddHours(1)) > modelo.FechaInicio);
        if (cruce)
            ModelState.AddModelError(nameof(modelo.FechaInicio), "Esta mascota ya tiene una reserva activa que se cruza con esas fechas.");

        if (!ModelState.IsValid)
        {
            var rehecho = await ArmarFormulario(User.IsInRole(RolUsuario.Cliente) ? idCliente : modelo.IdClienteSeleccionado);
            rehecho.IdClienteSeleccionado = modelo.IdClienteSeleccionado;
            rehecho.IdMascota = modelo.IdMascota;
            rehecho.IdTipoServicio = modelo.IdTipoServicio;
            rehecho.FechaInicio = modelo.FechaInicio;
            rehecho.FechaFin = modelo.FechaFin;
            rehecho.Observaciones = modelo.Observaciones;
            return View(rehecho);
        }

        var reserva = new Reserva
        {
            IdCliente = idCliente,
            IdMascota = modelo.IdMascota,
            IdTipoServicio = modelo.IdTipoServicio,
            FechaInicio = modelo.FechaInicio,
            FechaFin = modelo.FechaFin,
            Observaciones = modelo.Observaciones,
            EstadoReserva = EstadoReserva.Pendiente, // paso 7: el sistema registra la reserva
            FechaCreacion = DateTime.Now,
        };

        // Si la creó un empleado, queda asignado de una vez como responsable.
        if (User.IsInRole(RolUsuario.Empleado) && int.TryParse(User.FindFirstValue("id_empleado"), out var idEmp))
            reserva.IdEmpleado = idEmp;

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = User.IsInRole(RolUsuario.Cliente)
            ? "Tu reserva quedó registrada y está pendiente de confirmación."
            : "Reserva registrada correctamente en nombre del cliente.";
        return RedirectToAction(nameof(Index));
    }

    // ── AJAX: mascotas del cliente seleccionado (usado por Admin/Empleado en Create) ──
    [Authorize(Roles = $"{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpGet]
    public async Task<IActionResult> MascotasPorCliente(int idCliente)
    {
        var mascotas = await _db.Mascotas
            .Where(m => m.IdCliente == idCliente && m.Activo)
            .Select(m => new { id = m.IdMascota, nombre = m.Nombre })
            .ToListAsync();
        return Json(mascotas);
    }

    // ── Administrador/empleado confirman, inician o finalizan ────
    [Authorize(Roles = $"{RolUsuario.Administrador},{RolUsuario.Empleado}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        var estadosValidos = new[]
        {
            EstadoReserva.Pendiente, EstadoReserva.Confirmada,
            EstadoReserva.EnCurso, EstadoReserva.Finalizada, EstadoReserva.Cancelada
        };
        if (!estadosValidos.Contains(nuevoEstado)) return BadRequest();

        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva is null) return NotFound();

        reserva.EstadoReserva = nuevoEstado;

        // Al confirmar, asignar empleado y generar enlace de cámara
        if (nuevoEstado == EstadoReserva.Confirmada && reserva.IdEmpleado is null)
        {
            var idEmpleado = int.TryParse(User.FindFirstValue("id_empleado"), out var e) ? e : (int?)null;
            reserva.IdEmpleado = idEmpleado;
        }

        // Generar token de acceso a cámara al confirmar
        if (nuevoEstado == EstadoReserva.Confirmada)
        {
            var acceso = await CamaraController.GenerarToken(_db, reserva);
            await _db.SaveChangesAsync();

            var url = Url.Action("Ver", "Camara", new { token = acceso.Token }, Request.Scheme);
            TempData["CameraToken"] = acceso.Token;
            TempData["CameraUrl"]   = url;
            TempData["CameraMsg"]   = reserva.Mascota?.Nombre ?? "la mascota";
            TempData["Mensaje"] = $"Reserva #{id} confirmada. Se generó el enlace de acceso a las cámaras.";
            return RedirectToAction(nameof(Index));
        }

        await _db.SaveChangesAsync();
        TempData["Mensaje"] = $"Reserva #{id} actualizada a \"{nuevoEstado}\".";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ReservaFormViewModel> ArmarFormulario(int? idCliente)
    {
        var modelo = new ReservaFormViewModel
        {
            IdClienteSeleccionado = idCliente,
            FechaInicio = DateTime.Now.AddDays(1),
            ServiciosDisponibles = await _db.TiposServicio
                .Select(t => new TipoServicioOpcion { IdTipoServicio = t.IdTipoServicio, NombreServicio = t.NombreServicio, PrecioBase = t.PrecioBase })
                .ToListAsync(),
        };

        if (idCliente is not null)
        {
            modelo.MascotasDisponibles = await _db.Mascotas
                .Where(m => m.IdCliente == idCliente && m.Activo)
                .Select(m => new MascotaOpcion { IdMascota = m.IdMascota, Nombre = m.Nombre })
                .ToListAsync();
        }

        // Admin/Empleado necesitan elegir entre todos los clientes activos
        if (!User.IsInRole(RolUsuario.Cliente))
        {
            modelo.ClientesDisponibles = await _db.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new ClienteOpcion { IdCliente = c.IdCliente, NombreCompleto = c.Nombre + " " + c.Apellido })
                .ToListAsync();
        }

        return modelo;
    }
}
