using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("planilla_items")]
    public class PlanillaItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("planilla_id")]
        public int PlanillaId { get; set; }

        [ForeignKey("PlanillaId")]
        public Planilla? Planilla { get; set; }

        [Column("orden_proceso_id")]
        public int OrdenProcesoId { get; set; }

        [ForeignKey("OrdenProcesoId")]
        public OrdenProceso? OrdenProceso { get; set; }

        [Column("posicion")]
        public int Posicion { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "pendiente"; // 'pendiente', 'en_proceso', 'completado'

        public ICollection<RegistroSellado> Registros { get; set; } = new List<RegistroSellado>();
    }
}
