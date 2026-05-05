using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("pedido_historial_estado")]
    public class PedidoHistorialEstado
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [Column("estado_anterior")]
        public string? EstadoAnterior { get; set; }

        [Column("estado_nuevo")]
        public string EstadoNuevo { get; set; } = string.Empty;

        [Column("usuario_id")]
        public Guid? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Column("observacion")]
        public string? Observacion { get; set; }
    }
}
