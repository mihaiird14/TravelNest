using MimeKit.Cryptography;
using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class LikesPostare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        public int PostareId { set; get; }
        public Postare Postare { set; get; }   
        public string UserId { set; get; }
        public ApplicationUser User { set; get; }
    }
}
