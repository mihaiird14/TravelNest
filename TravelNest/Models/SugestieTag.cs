using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelNest.Models;

public class SugestieTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int FaceEmbeddingId { get; set; }
    public FaceEmbeddings FaceEmbedding { get; set; }

    public int SuggestedPersonId { get; set; }
    public Profil SuggestedPerson { get; set; }

    public double Confidence { get; set; }
}
