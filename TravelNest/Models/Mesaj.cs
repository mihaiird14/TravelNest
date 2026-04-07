using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class Mesaj
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ContinutContent { get; set; } = string.Empty;
        public DateTime DataTrimite { get; set; } = DateTime.Now;

        public int ExpeditorProfilId { get; set; }
        public Profil Expeditor { get; set; } = null!;

        // Ptr chat 1-1
        public int? DestinatarProfilId { get; set; }
        public Profil? Destinatar { get; set; }

        // Ptr grup
        public int? TravelGroupId { get; set; }
        public TravelGroup? TravelGroup { get; set; }
        public virtual ICollection<VizualizareMesaj> VizualizariMessages { get; set; } = new List<VizualizareMesaj>();
    }
}