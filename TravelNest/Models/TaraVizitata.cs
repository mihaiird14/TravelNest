using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class TaraVizitata
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProfilId { get; set; }

        [ForeignKey("ProfilId")]
        public Profil Profil { get; set; }
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CodTara { get; set; }
        public bool AdaugatManual { get; set; }
    }
}
