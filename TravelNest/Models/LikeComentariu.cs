using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class LikeComentariu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ComentariuId { get; set; }
        public Comentariu Comentariu { get; set; }
        public int ProfilId { get; set; }
        public Profil Profil { get; set; }
    }
}
