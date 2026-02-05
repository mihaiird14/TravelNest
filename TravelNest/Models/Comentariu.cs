using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class Comentariu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "Comentariul nu poate depăși 500 de caractere.")]
        public string Continut { get; set; }

        public DateTime DataCr { get; set; } = DateTime.UtcNow; 
        public int PostareId { get; set; }

        [ForeignKey("PostareId")]
        public Postare Postare { get; set; }
        public int ProfilId { get; set; }

        [ForeignKey("ProfilId")]
        public Profil Profil { get; set; }
        public List<ReplyCom> Raspunsuri { get; set; } = new List<ReplyCom>();
        public bool ComentariuEditat { get; set; } = false;
    }
}