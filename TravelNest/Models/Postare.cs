using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class Postare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        [Required]
        public int CreatorId { set; get; }
        public Profil Profil { set; get; }
        [MaxLength(500)]
        public string? Descriere { set; get; }
        [MaxLength(255)]
        public string? Locatie { set; get; }
        public List<string> UseriMentionati { get; set; } = new List<string>();
        public List<FisierMedia> FisiereMedia { get; set; } = new List<FisierMedia>();
        public List<Comentariu> Comentarii { get; set; } = new List<Comentariu>();
        public List<LikesPostare> Likes { get; set; } = new List<LikesPostare>();
        public DateTime DataCr { get; set; } = DateTime.UtcNow;
    }
}
