using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class Cheltuiala
    {
        [Key]
        public int Id { get; set; }

        public int TravelGroupId { get; set; }
        [ForeignKey("TravelGroupId")]
        public TravelGroup TravelGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string Titlu { get; set; }

        public decimal SumaTotala { get; set; }

        public string Moneda { get; set; } = "EUR";

        public bool EsteAutomata { get; set; } = false;

        public ICollection<PlataMembru> PlatiMembri { get; set; } = new List<PlataMembru>();
    }
}