using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

[Authorize(Roles = $"{RolUsuario.Administrador},{RolUsuario.Empleado}")]
public class EmpleadosController : Controller
{
    private readonly VacanJefContext _db;
    public EmpleadosController(VacanJefContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var empleados = await _db.Empleados
            .Include(e => e.Cargo)
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        var ahora    = DateTime.Now;
        var diaHoy   = AbreviarDia(ahora.DayOfWeek);
        var horaHoy  = ahora.TimeOfDay;

        var modelo = empleados.Select(e =>
        {
            var diasLista = (e.DiasLaborales ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            var trabajaHoy = diasLista.Contains(diaHoy);
            var enTurno = trabajaHoy && e.TurnoInicio is not null && e.TurnoFin is not null
                          && horaHoy >= e.TurnoInicio && horaHoy <= e.TurnoFin;

            return new EmpleadoTurnoViewModel
            {
                IdEmpleado    = e.IdEmpleado,
                Nombre        = e.Nombre,
                Apellido      = e.Apellido,
                Correo        = e.Correo,
                Telefono      = e.Telefono,
                Cargo         = e.Cargo?.NombreCargo ?? "—",
                TurnoInicio   = e.TurnoInicio,
                TurnoFin      = e.TurnoFin,
                DiasLaborales = diasLista.ToList(),
                EnTurnoAhora  = enTurno,
                TrabajaHoy    = trabajaHoy,
            };
        }).ToList();

        ViewBag.DiaHoy = diaHoy;
        return View(modelo);
    }

    private static string AbreviarDia(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday    => "Lun",
        DayOfWeek.Tuesday   => "Mar",
        DayOfWeek.Wednesday => "Mie",
        DayOfWeek.Thursday  => "Jue",
        DayOfWeek.Friday    => "Vie",
        DayOfWeek.Saturday  => "Sab",
        _                   => "Dom",
    };
}
