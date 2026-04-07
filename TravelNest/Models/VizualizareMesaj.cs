using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class VizualizareMesaj
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MesajId { get; set; }
        public Mesaj Mesaj { get; set; } = null!;
        public int ProfilId { get; set; }
        public Profil Profil { get; set; } = null!;

        public DateTime DataSeen { get; set; } = DateTime.Now;
    }
}