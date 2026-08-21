using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;

namespace VacanJefWeb.Controllers;

public class CamaraController : Controller
{
    private readonly VacanJefContext _db;
    public CamaraController(VacanJefContext db) => _db = db;

    // ── Página pública: no requiere login, solo el token ─────────
    // URL: /camara/{token}
    public async Task<IActionResult> Ver(string token)
    {
        var acceso = await _db.AccesosCamara
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Mascota)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Cliente)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Empleado)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.TipoServicio)
            .FirstOrDefaultAsync(a => a.Token == token);

        if (acceso is null)
            return View("TokenInvalido", (string?)"El enlace no existe.");

        if (!acceso.Activo)
            return View("TokenInvalido", (string?)"Este enlace fue desactivado.");

        if (acceso.FechaExpiracion < DateTime.Now)
            return View("TokenInvalido", (string?)"Este enlace expiró.");

        return View(acceso);
    }

    // ── Generar token para una reserva (llamado al confirmarla) ──
    public static async Task<AccesoCamara> GenerarToken(
        VacanJefContext db, Reserva reserva)
    {
        // Si ya tiene token activo, devolverlo
        var existente = await db.AccesosCamara
            .FirstOrDefaultAsync(a => a.IdReserva == reserva.IdReserva && a.Activo);
        if (existente is not null) return existente;

        var token = Guid.NewGuid().ToString("N"); // 32 hex chars, URL-safe
        var acceso = new AccesoCamara
        {
            IdReserva        = reserva.IdReserva,
            Token            = token,
            FechaCreacion    = DateTime.Now,
            FechaExpiracion  = reserva.FechaFin ?? DateTime.Now.AddDays(30),
            Activo           = true,
        };
        db.AccesosCamara.Add(acceso);
        return acceso;
    }
}
