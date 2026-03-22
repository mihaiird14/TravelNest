using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class VizualizarePostare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PostareId { get; set; }
        public virtual Postare Postare { get; set; }

        public int VisitorProfilId { get; set; } 
        public DateTime DataVizualizare { get; set; } = DateTime.Now;
    }
}
