using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

public class RegistroController : Controller
{
    private readonly VacanJefContext _db;
    public RegistroController(VacanJefContext db) => _db = db;

    // ══════════════════════════════════════════════════════════════
    // REGISTRO DE CLIENTE (público, sin login)
    // ══════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Cliente() => View(new RegistroClienteViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cliente(RegistroClienteViewModel modelo)
    {
        // Verificar correo único
        if (await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == modelo.Correo.ToLower()))
        {
            ModelState.AddModelError(nameof(modelo.Correo), "Este correo ya está registrado.");
        }

        if (!ModelState.IsValid) return View(modelo);

        // 1. Crear fila en Clientes
        var cliente = new Cliente
        {
            Nombre        = modelo.Nombre.Trim(),
            Apellido      = modelo.Apellido.Trim(),
            Telefono      = modelo.Telefono?.Trim(),
            Direccion     = modelo.Direccion?.Trim(),
            Correo        = modelo.Correo.Trim().ToLower(),
            FechaRegistro = DateTime.Now,
            Activo        = true,
        };
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(); // para obtener el id_cliente generado

        // 2. Crear contacto de emergencia si lo ingresaron
        if (!string.IsNullOrWhiteSpace(modelo.ContactoNombre))
        {
            _db.ContactosEmergencia.Add(new ContactoEmergencia
            {
                IdCliente   = cliente.IdCliente,
                Nombre      = modelo.ContactoNombre.Trim(),
                Apellido    = modelo.ContactoApellido?.Trim() ?? "",
                Telefono    = modelo.ContactoTelefono?.Trim(),
                Parentesco  = modelo.ContactoParentesco?.Trim(),
            });
        }

        // 3. Crear usuario en tabla Usuarios con bcrypt
        _db.Usuarios.Add(new Usuario
        {
            Correo        = modelo.Correo.Trim().ToLower(),
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(modelo.Password),
            Rol           = RolUsuario.Cliente,
            IdCliente     = cliente.IdCliente,
            Activo        = true,
            FechaCreacion = DateTime.Now,
        });

        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Cuenta creada con éxito. Ya puedes iniciar sesión, {modelo.Nombre}.";
        return RedirectToAction("Login", "Cuenta");
    }

    // ══════════════════════════════════════════════════════════════
    // REGISTRO DE EMPLEADO (solo Administrador)
    // ══════════════════════════════════════════════════════════════
    [Authorize(Roles = RolUsuario.Administrador)]
    [HttpGet]
    public async Task<IActionResult> Empleado()
    {
        var modelo = new RegistroEmpleadoViewModel
        {
            CargosDisponibles = await ObtenerCargos(),
        };
        return View(modelo);
    }

    [Authorize(Roles = RolUsuario.Administrador)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Empleado(RegistroEmpleadoViewModel modelo)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Correo.ToLower() == modelo.Correo.ToLower()))
            ModelState.AddModelError(nameof(modelo.Correo), "Este correo ya está registrado.");

        if (!ModelState.IsValid)
        {
            modelo.CargosDisponibles = await ObtenerCargos();
            return View(modelo);
        }

        // 1. Crear fila en Empleados
        var empleado = new Empleado
        {
            Nombre                 = modelo.Nombre.Trim(),
            Apellido               = modelo.Apellido.Trim(),
            Telefono               = modelo.Telefono?.Trim(),
            Direccion              = modelo.Direccion?.Trim(),
            Correo                 = modelo.Correo.Trim().ToLower(),
            IdCargo                = modelo.IdCargo,
            Salario                = modelo.Salario,
            FechaIngreso           = modelo.FechaIngreso,
            EmergenciaNombre       = modelo.EmergenciaNombre?.Trim(),
            EmergenciaTelefono     = modelo.EmergenciaTelefono?.Trim(),
            EmergenciaParentesco   = modelo.EmergenciaParentesco?.Trim(),
            Activo                 = true,
        };
        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();

        // 2. Crear usuario
        _db.Usuarios.Add(new Usuario
        {
            Correo        = modelo.Correo.Trim().ToLower(),
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(modelo.Password),
            Rol           = RolUsuario.Empleado,
            IdEmpleado    = empleado.IdEmpleado,
            Activo        = true,
            FechaCreacion = DateTime.Now,
        });
        await _db.SaveChangesAsync();

        TempData["Mensaje"] = $"Empleado {modelo.Nombre} {modelo.Apellido} registrado correctamente.";
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task<List<CargoOpcion>> ObtenerCargos() =>
        await _db.Cargos
            .OrderBy(c => c.NombreCargo)
            .Select(c => new CargoOpcion { IdCargo = c.IdCargo, NombreCargo = c.NombreCargo })
            .ToListAsync();
}
