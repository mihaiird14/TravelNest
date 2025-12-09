using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class FaceEmbeddings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int FisierMediaId { get; set; }
        public FisierMedia FisierMedia { get; set; }
        public int? PersonId { get; set; }
        public string Embedding { get; set; }
        public Profil Person { get; set; }
    }
}
