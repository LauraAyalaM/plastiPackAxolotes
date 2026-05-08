using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("planillas")]
    public class Planilla
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

       [Column("selladora_id")]
        public int SelladoraId { get; set; }

        [ForeignKey("SelladoraId")]
        public Selladora? Selladora { get; set; }

        [Column("fecha")]
        public DateOnly Fecha { get; set; }
        [Column("creado_por")]
        public Guid? CreadoPor { get; set; }

        [ForeignKey("CreadoPor")]
        public Usuario? UsuarioCreador { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PlanillaItem> Items { get; set; } = new List<PlanillaItem>();
    }
}
