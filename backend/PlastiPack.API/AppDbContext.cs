using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Models;

namespace PlastiPack.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Referencia> Referencias { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<OrdenProduccion> OrdenesProduccion { get; set; }
        public DbSet<Selladora> Selladoras { get; set; }
        public DbSet<Planilla> Planillas { get; set; }
        public DbSet<RegistroSellado> RegistrosSellado { get; set; }
    }
}