using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class ReplyCom
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public Profil User { get; set; }
        public string Mesaj { get; set; }
        public int ComentariuId { get; set; }
        public Comentariu Comentariu { get; set; }
        public DateTime DataPost { get; set; } = DateTime.UtcNow;
        public bool RaspunsEditat { get; set; } = false;
    }
}
