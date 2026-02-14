using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class MembruGrup
    {
        public int ProfilId { get; set; }
        public Profil Profil { get; set; }

        public int TravelGroupId { get; set; }
        public TravelGroup TravelGroup { get; set; }

        public DateTime DataInscrierii { get; set; } = DateTime.Now;
    }
}
