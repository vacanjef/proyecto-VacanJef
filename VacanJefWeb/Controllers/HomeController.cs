using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;

namespace VacanJefWeb.Controllers;

// Página pública: no requiere [Authorize]. Es la nueva raíz del sitio.
public class HomeController : Controller
{
    private readonly VacanJefContext _db;
    public HomeController(VacanJefContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Servicios y precios reales, tal como están cargados en la BD ahora mismo.
        ViewBag.Servicios = await _db.TiposServicio.OrderBy(t => t.PrecioBase).ToListAsync();
        return View();
    }
}
