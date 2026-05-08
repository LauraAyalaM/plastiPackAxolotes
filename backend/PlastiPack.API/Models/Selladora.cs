using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("selladoras")]
    public class Selladora
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("activa")]
        public bool Activa { get; set; } = true;
        public ICollection<Planilla> Planillas { get; set; } = new List<Planilla>();

    }
}
