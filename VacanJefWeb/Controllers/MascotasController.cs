using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize]
public class MascotasController : Controller
{
    private readonly VacanJefContext _db;
    public MascotasController(VacanJefContext db) => _db = db;

    // Cliente ve solo las suyas; Admin/Empleado ven todas.
    public async Task<IActionResult> Index()
    {
        var query = _db.Mascotas.Include(m => m.Cliente).Where(m => m.Activo).AsQueryable();

        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");
            query = query.Where(m => m.IdCliente == idCliente);
        }

        return View(await query.OrderBy(m => m.Nombre).ToListAsync());
    }

    public IActionResult Create()
    {
        var modelo = new MascotaFormViewModel();
        if (!User.IsInRole(RolUsuario.Cliente))
            ViewBag.Clientes = _db.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MascotaFormViewModel modelo)
    {
        // Flujo básico CU-01, paso 5: el sistema valida los datos ingresados.
        if (!ModelState.IsValid)
        {
            if (!User.IsInRole(RolUsuario.Cliente))
                ViewBag.Clientes = _db.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
            return View(modelo);
        }

        int idCliente;
        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idClienteSesion) || idClienteSesion <= 0)
                return RedirectToAction("Login", "Cuenta");
            idCliente = idClienteSesion;
        }
        else
        {
            // Flujo alterno 6a: el administrador/empleado registra la mascota manualmente.
            if (modelo.IdClienteSeleccionado is null)
            {
                ModelState.AddModelError(string.Empty, "Selecciona el dueño de la mascota.");
                ViewBag.Clientes = _db.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
                return View(modelo);
            }
            idCliente = modelo.IdClienteSeleccionado.Value;
        }

        var mascota = new Mascota
        {
            IdCliente = idCliente,
            Nombre = modelo.Nombre,
            Especie = modelo.Especie,
            Raza = modelo.Raza,
            Sexo = modelo.Sexo,
            FechaNacimiento = modelo.FechaNacimiento,
            Color = modelo.Color,
            PesoKg = modelo.PesoKg,
            Alergias = modelo.Alergias,
            Medicamentos = modelo.Medicamentos,
            Observaciones = modelo.Observaciones,
            CarnetVacunas = modelo.CarnetVacunas,
            Activo = true,
        };

        _db.Mascotas.Add(mascota);
        await _db.SaveChangesAsync(); // paso 6: genera el perfil único (id_mascota)

        TempData["Mensaje"] = $"{mascota.Nombre} quedó registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var mascota = await _db.Mascotas.Include(m => m.Cliente)
            .Include(m => m.Reservas).ThenInclude(r => r.TipoServicio)
            .FirstOrDefaultAsync(m => m.IdMascota == id);

        if (mascota is null) return NotFound();

        if (User.IsInRole(RolUsuario.Cliente))
        {
            if (!int.TryParse(User.FindFirstValue("id_cliente"), out var idCliente) || idCliente <= 0)
                return RedirectToAction("Login", "Cuenta");
            if (mascota.IdCliente != idCliente) return Forbid();
        }

        return View(mascota);
    }
}
