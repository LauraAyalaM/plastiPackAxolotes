using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("ordenes_produccion")]
    public class OrdenProduccion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("referencia_id")]
        public int ReferenciaId { get; set; }

        [ForeignKey("ReferenciaId")]
        public Referencia? Referencia { get; set; }

        [Column("cantidad_requerida")]
        public int CantidadRequerida { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("creado_por")]
        public Guid? CreadoPor { get; set; }

        [ForeignKey("CreadoPor")]
        public Usuario? UsuarioCreador { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrdenProceso> Procesos { get; set; } = new List<OrdenProceso>();
    }
}
