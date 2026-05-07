using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;

namespace PlastiPack.API.Controllers
{
    [Authorize(Roles = "jefe_produccion")]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol!.Nombre == "vendedor" || u.Rol!.Nombre == "operario")
                .OrderBy(u => u.Rol!.Nombre)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

            ViewData["ActivePage"] = "Usuarios";
            return View(usuarios);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo = !usuario.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario '{usuario.Nombre}' {(usuario.Activo ? "activado" : "desactivado")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}