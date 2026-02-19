using Microsoft.AspNetCore.Routing.Constraints;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class TravelGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Nume { get; set; }    
        [Required]
        public int AdminId { get; set; }
        public Profil Admin { get; set; }
        public string? Descriere { get; set; }

        public DateOnly? DataPlecare { get; set; }
        public DateOnly? DataIntoarcere { get; set; }
        public string? Thumbnail { get; set; }
        public List<MembruGrup> ListaParticipanti { get; set; } = new List<MembruGrup>();
        public List<LocatieGrup> Locatii { get; set; } = new List<LocatieGrup>();
        public List<DocumenteTG> Documente { get; set; } = new List<DocumenteTG>();
    }
}
