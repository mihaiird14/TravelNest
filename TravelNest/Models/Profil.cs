using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelNest.Data;

namespace TravelNest.Models
{
    public class Profil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        
        public string UserId { set; get; }
        [ValidateNever]
        public ApplicationUser User { set; get; }
        
        public string? ImagineProfil { set; get; } = "/images/profilDefault.png";

        [MaxLength(255)]
        public string? Bio { set; get; }
        public List<Postare> Posts { get; set; } = new List<Postare>();
        public List<FaceEmbeddings> FaceEmbeddings { get; set; }
        public List<ReplyCom> ReplyComs { get; set; } = new List<ReplyCom>();
        public List<LikeComentariu> LikeComentarii { get; set; } = new List<LikeComentariu>();
        public List<LikeReplyComentarii> LikeReplyComentarii { get; set; } = new List<LikeReplyComentarii>();
        public bool isPrivate { get; set; } = false;
        public bool autoTag { get; set; } = false;
        public bool manualTag { get; set; } = false;
    }
}
