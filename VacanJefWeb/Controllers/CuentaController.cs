using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacanJefWeb.Data;
using VacanJefWeb.Models.ViewModels;

namespace VacanJefWeb.Controllers;

public class CuentaController : Controller
{
    private readonly VacanJefContext _db;

    public CuentaController(VacanJefContext db) => _db = db;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard(User.FindFirstValue(ClaimTypes.Role));

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var correo = modelo.Correo.Trim().ToLower();
        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo && u.Activo);

        // El mismo mensaje genérico para correo inexistente o contraseña incorrecta,
        // para no revelar qué correos están registrados.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(modelo.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol),
        };
        if (usuario.IdCliente is not null) claims.Add(new Claim("id_cliente", usuario.IdCliente.Value.ToString()));
        if (usuario.IdEmpleado is not null) claims.Add(new Claim("id_empleado", usuario.IdEmpleado.Value.ToString()));

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            new AuthenticationProperties
            {
                IsPersistent = modelo.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            return Redirect(modelo.ReturnUrl);

        return RedirectToDashboard(usuario.Rol);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Error() => View();

    private IActionResult RedirectToDashboard(string? rol) => rol switch
    {
        Models.RolUsuario.Administrador => RedirectToAction("Admin", "Dashboard"),
        Models.RolUsuario.Empleado => RedirectToAction("Empleado", "Dashboard"),
        _ => RedirectToAction("Cliente", "Dashboard"),
    };
}
