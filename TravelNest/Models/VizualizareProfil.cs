using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class VizualizareProfil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TargetProfilId { get; set; }
        public int VisitorProfilId { get; set; }
        public DateTime DataVizualizare { get; set; } = DateTime.Now;
    }
}
