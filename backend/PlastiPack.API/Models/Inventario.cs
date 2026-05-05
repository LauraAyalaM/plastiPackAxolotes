using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("inventario")]
    public class Inventario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("referencia_id")]
        public int ReferenciaId { get; set; }

        [ForeignKey("ReferenciaId")]
        public Referencia? Referencia { get; set; }

        [Column("stock_disponible")]
        public int StockDisponible { get; set; } = 0;

        [Column("stock_reservado")]
        public int StockReservado { get; set; } = 0;

        [Column("ultima_actualizacion")]
        public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
