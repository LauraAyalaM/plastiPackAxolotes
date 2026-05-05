using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // ── LISTADO ──
        public async Task<IActionResult> Index(
            string? estado,
            string? destino,
            string? busqueda,
            int pagina = 1)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .AsQueryable();

            // Vendedor solo ve sus propios pedidos
            if (User.IsInRole("vendedor"))
            {
                var userId = Guid.Parse(User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                query = query.Where(p => p.VendedorId == userId);
            }

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrWhiteSpace(destino))
                query = query.Where(p => p.Destino == destino);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(p =>
                    (p.Cliente != null && p.Cliente.Nombre.Contains(busqueda)) ||
                    p.Id.ToString().Contains(busqueda));

            var total = await query.CountAsync();
            var pedidos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .ThenByDescending(p => p.Id)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Estado       = estado;
            ViewBag.Destino      = destino;
            ViewBag.Busqueda     = busqueda;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total        = total;

            ViewData["ActivePage"] = "Pedidos";
            return View(pedidos);
        }

        // ── DETALLE ──
        public async Task<IActionResult> Detalle(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Vendedor)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Referencia)
                .Include(p => p.Historial)
                    .ThenInclude(h => h.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // Vendedor solo puede ver sus pedidos
            if (User.IsInRole("vendedor"))
            {
                var userId = Guid.Parse(User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                if (pedido.VendedorId != userId) return Forbid();
            }

            ViewData["ActivePage"] = "Pedidos";
            return View(pedido);
        }

        // ── CREAR GET ──
        [Authorize(Roles = "vendedor,jefe_produccion")]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewData["ActivePage"] = "Pedidos";
            return View(new Pedido());
        }

        // ── CREAR POST ──
        [HttpPost]
        [Authorize(Roles = "vendedor,jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            Pedido model,
            List<int> referenciaIds,
            List<int> cantidades,
            List<decimal> precios)
        {
            // ── Validación: fecha mínima 15 días ──
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            if (DateOnly.FromDateTime(model.FechaEntrega) < hoy.AddDays(15))
            {
                ModelState.AddModelError("FechaEntrega",
                    "La fecha de entrega debe ser al menos 15 días desde hoy.");
            }

            // ── Validación: destino externo requiere cliente ──
            if (model.Destino == "externo" && (model.ClienteId == null || model.ClienteId == 0))
            {
                ModelState.AddModelError("ClienteId",
                    "Los pedidos externos requieren un cliente.");
            }

            // ── Validación: al menos un ítem ──
            if (referenciaIds == null || referenciaIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty,
                    "El pedido debe tener al menos un ítem.");
            }

            // ── Validación: stock disponible ──
            var erroresStock = new List<string>();
            if (referenciaIds != null)
            {
                for (int i = 0; i < referenciaIds.Count; i++)
                {
                    var inv = await _context.Inventario
                        .Include(inv => inv.Referencia)
                        .FirstOrDefaultAsync(inv => inv.ReferenciaId == referenciaIds[i]);

                    if (inv == null) continue;

                    var disponible = inv.StockDisponible - inv.StockReservado;
                    if (cantidades[i] > disponible)
                    {
                        erroresStock.Add(
                            $"'{inv.Referencia?.Codigo}': solicitado {cantidades[i]}, disponible {disponible}.");
                    }
                }
            }

            if (erroresStock.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Stock insuficiente para: " + string.Join(" | ", erroresStock));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = await _context.Clientes
                    .Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
                ViewData["ActivePage"] = "Pedidos";
                return View(model);
            }

            // ── Asignar vendedor ──
            var vendedorId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            model.VendedorId  = vendedorId;
            model.FechaCreacion = DateTime.Today;
            model.Estado      = "pendiente";
            model.CreatedAt   = DateTime.UtcNow;

            // Cliente nulo para pedidos internos
            if (model.Destino == "interno") model.ClienteId = null;

            _context.Pedidos.Add(model);
            await _context.SaveChangesAsync();

            // ── Guardar ítems y reservar stock ──
            for (int i = 0; i < referenciaIds.Count; i++)
            {
                _context.PedidoDetalles.Add(new PedidoDetalle
                {
                    PedidoId     = model.Id,
                    ReferenciaId = referenciaIds[i],
                    Cantidad     = cantidades[i],
                    Precio       = precios[i]
                });

                // Reservar en inventario
                var inv = await _context.Inventario
                    .FirstOrDefaultAsync(inv => inv.ReferenciaId == referenciaIds[i]);
                if (inv != null)
                {
                    inv.StockReservado += cantidades[i];
                    inv.UltimaActualizacion = DateTime.UtcNow;
                }
            }

            // ── Historial ──
            _context.PedidoHistorial.Add(new PedidoHistorialEstado
            {
                PedidoId    = model.Id,
                EstadoNuevo = "pendiente",
                UsuarioId   = vendedorId,
                Observacion = "Pedido creado"
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Pedido #{model.Id} creado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = model.Id });
        }

        // ── CAMBIAR ESTADO ──
        [HttpPost]
        [Authorize(Roles = "jefe_produccion,vendedor")]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado, string? observacion)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            // Vendedor solo puede cancelar sus propios pedidos pendientes
            if (User.IsInRole("vendedor"))
            {
                var userId = Guid.Parse(User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                if (pedido.VendedorId != userId) return Forbid();
                if (nuevoEstado != "cancelado")  return Forbid();
                if (pedido.Estado != "pendiente")
                {
                    TempData["Error"] = "Solo se pueden cancelar pedidos en estado pendiente.";
                    return RedirectToAction(nameof(Detalle), new { id });
                }
            }

            var estadoAnterior = pedido.Estado;
            pedido.Estado = nuevoEstado;

            // Si se cancela, liberar el stock reservado
            if (nuevoEstado == "cancelado")
            {
                var detalles = await _context.PedidoDetalles
                    .Where(d => d.PedidoId == id)
                    .ToListAsync();

                foreach (var det in detalles)
                {
                    var inv = await _context.Inventario
                        .FirstOrDefaultAsync(inv => inv.ReferenciaId == det.ReferenciaId);
                    if (inv != null)
                    {
                        inv.StockReservado = Math.Max(0, inv.StockReservado - det.Cantidad);
                        inv.UltimaActualizacion = DateTime.UtcNow;
                    }
                }
            }

            var actorId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            _context.PedidoHistorial.Add(new PedidoHistorialEstado
            {
                PedidoId       = id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo    = nuevoEstado,
                UsuarioId      = actorId,
                Observacion    = observacion
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Pedido #{id} pasó a '{nuevoEstado}'.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // ── API: precio sugerido para referencia + cliente ──
        [HttpGet]
        public async Task<IActionResult> PrecioSugerido(int referenciaId, int? clienteId)
        {
            // Determina categoría según tipo de cliente
            string categoria = "Lista";
            if (clienteId.HasValue)
            {
                var cliente = await _context.Clientes.FindAsync(clienteId.Value);
                if (cliente?.Tipo == "interno") categoria = "Mayorista";
                else categoria = "Mostrador";
            }

            var precio = await _context.PreciosReferencia
                .Where(p => p.ReferenciaId == referenciaId && p.Categoria == categoria)
                .OrderByDescending(p => p.VigenteDesde)
                .Select(p => p.Precio)
                .FirstOrDefaultAsync();

            var stock = await _context.Inventario
                .Where(i => i.ReferenciaId == referenciaId)
                .Select(i => i.StockDisponible - i.StockReservado)
                .FirstOrDefaultAsync();

            return Json(new { precio, stock, categoria });
        }
    }
}