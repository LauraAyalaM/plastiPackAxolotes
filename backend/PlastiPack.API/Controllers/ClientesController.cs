using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // ── LISTADO ──
        public async Task<IActionResult> Index(string? busqueda, string? tipo, int pagina = 1)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(c =>
                    c.Nombre.Contains(busqueda) ||
                    (c.Nit != null && c.Nit.Contains(busqueda)) ||
                    (c.Email != null && c.Email.Contains(busqueda)));

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(c => c.Tipo == tipo);

            var total = await query.CountAsync();
            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Busqueda     = busqueda;
            ViewBag.Tipo         = tipo;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total        = total;

            ViewData["ActivePage"] = "Clientes";
            return View(clientes);
        }

        // ── CREAR GET ──
        [Authorize(Roles = "vendedor,jefe_produccion")]
        public IActionResult Crear()
        {
            ViewData["ActivePage"] = "Clientes";
            return View(new Cliente());
        }

        // ── CREAR POST ──
        [HttpPost]
        [Authorize(Roles = "vendedor,jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cliente model)
        {
            // NIT duplicado
            if (!string.IsNullOrWhiteSpace(model.Nit) &&
                await _context.Clientes.AnyAsync(c => c.Nit == model.Nit))
            {
                ModelState.AddModelError("Nit", "Ya existe un cliente con ese NIT.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ActivePage"] = "Clientes";
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.Clientes.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Cliente '{model.Nombre}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDITAR GET ──
        [Authorize(Roles = "vendedor,jefe_produccion")]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            ViewData["ActivePage"] = "Clientes";
            return View(cliente);
        }

        // ── EDITAR POST ──
        [HttpPost]
        [Authorize(Roles = "vendedor,jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cliente model)
        {
            if (id != model.Id) return BadRequest();

            // NIT duplicado (excluir el mismo cliente)
            if (!string.IsNullOrWhiteSpace(model.Nit) &&
                await _context.Clientes.AnyAsync(c => c.Nit == model.Nit && c.Id != id))
            {
                ModelState.AddModelError("Nit", "Ya existe otro cliente con ese NIT.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ActivePage"] = "Clientes";
                return View(model);
            }

            var existente = await _context.Clientes.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre    = model.Nombre;
            existente.Nit       = model.Nit;
            existente.Telefono  = model.Telefono;
            existente.Email     = model.Email;
            existente.Direccion = model.Direccion;
            existente.Tipo      = model.Tipo;
            existente.Activo    = model.Activo;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Cliente '{existente.Nombre}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── CAMBIAR ESTADO ──
        [HttpPost]
        [Authorize(Roles = "vendedor,jefe_produccion")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Activo = !cliente.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Cliente '{cliente.Nombre}' {(cliente.Activo ? "activado" : "desactivado")}.";
            return RedirectToAction(nameof(Index));
        }

        // ── BUSQUEDA AJAX (para selector en pedidos) ──
        [HttpGet]
        public async Task<IActionResult> Buscar(string q)
        {
            var resultados = await _context.Clientes
                .Where(c => c.Activo &&
                           (c.Nombre.Contains(q) ||
                            (c.Nit != null && c.Nit.Contains(q))))
                .Take(10)
                .Select(c => new { c.Id, c.Nombre, c.Nit, c.Tipo })
                .ToListAsync();

            return Json(resultados);
        }
    }
}