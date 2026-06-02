using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class PlataMembru
    {
        [Key]
        public int Id { get; set; }

        public int CheltuialaId { get; set; }
        [ForeignKey("CheltuialaId")]
        public Cheltuiala Cheltuiala { get; set; }

        public int ProfilId { get; set; }
        [ForeignKey("ProfilId")]
        public Profil Profil { get; set; }

        public decimal SumaDatorata { get; set; } 

        public bool EstePlatit { get; set; } = false; 
    }
}