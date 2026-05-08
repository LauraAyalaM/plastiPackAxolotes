using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var esJefe     = User.IsInRole("jefe_produccion");
            var esVendedor = User.IsInRole("vendedor");
            var esOperario = User.IsInRole("operario");
            var userId     = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var hoy        = DateOnly.FromDateTime(DateTime.UtcNow);

            if (esJefe)
            {
                ViewBag.TotalPedidosEnProduccion = await _context.Pedidos
                    .CountAsync(p => p.Estado == "en_produccion");

                ViewBag.TotalOrdenesPendientes = await _context.OrdenesProduccion
                    .CountAsync(o => o.Estado == "pendiente");

                ViewBag.SelladoresConPlanillaHoy = await _context.Planillas
                    .CountAsync(p => p.Fecha == hoy);

                ViewBag.UltimosPedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                ViewBag.OrdenesPorEstado = await _context.OrdenesProduccion
                    .GroupBy(o => o.Estado)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }

            if (esVendedor)
            {
                ViewBag.MisPedidosActivos = await _context.Pedidos
                    .CountAsync(p => p.VendedorId == userId && p.Estado == "en_produccion");

                ViewBag.MisPedidosEntregadosMes = await _context.Pedidos
                    .CountAsync(p => p.VendedorId == userId
                                  && p.Estado == "entregado"
                                  && p.FechaCreacion.Month == DateTime.UtcNow.Month
                                  && p.FechaCreacion.Year  == DateTime.UtcNow.Year);

                ViewBag.MisPedidosRecientes = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Where(p => p.VendedorId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }

            if (esOperario)
            {
                var planilla = await _context.Planillas
                    .Include(p => p.Selladora)
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.Fecha == hoy);

                ViewBag.PlanillaHoy     = planilla;
                ViewBag.PlanillaHoyId   = planilla?.Id;
                ViewBag.NombreSelladora = planilla?.Selladora?.Nombre;
                ViewBag.TotalTareasHoy  = planilla?.Items?.Count ?? 0;
            }

            ViewData["ActivePage"] = "Dashboard";
            return View();
        }
    }
}