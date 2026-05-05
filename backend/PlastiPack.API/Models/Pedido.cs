using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("pedidos")]
    public class Pedido
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_entrega")]
        public DateTime FechaEntrega { get; set; }

        [Column("cliente_id")]
        public int? ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Column("vendedor_id")]
        public Guid VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Usuario? Vendedor { get; set; }

        [Column("destino")]
        public string Destino { get; set; } = string.Empty; // 'externo' o 'interno'

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
        public ICollection<PedidoHistorialEstado> Historial { get; set; } = new List<PedidoHistorialEstado>();
    }
}
