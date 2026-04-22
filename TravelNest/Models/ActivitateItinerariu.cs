using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class ActivitateItinerariu
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TravelGroupId { get; set; }
        public virtual TravelGroup TravelGroup { get; set; }

        public int Zi { get; set; } 
        public string Ora { get; set; } 
        public string Titlu { get; set; }
        public string Descriere { get; set; }
        public bool GeneratAI { get; set; } = false;
    }
}
