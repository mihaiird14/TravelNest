using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public enum Tip
    {
        Image,
        Video
    }
    public class FisierMedia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        [Required]
        public string Url { set; get; } = string.Empty;
        [Required]
        public Tip fisier { set; get; }

        public int PostareId { set; get; }
        public Postare Postare { set; get; }
        public List<FaceEmbeddings> FaceEmbeddings { get; set; }
    }
}
