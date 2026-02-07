using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class LikeReplyComentarii
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ReplyId { get; set; }
        public int ProfilId { get; set; }
        public Profil Profil { get; set; }
        public ReplyCom ReplyCom { get; set; }
    }
}
