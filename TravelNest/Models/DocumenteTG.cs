using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class DocumenteTG
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string NumeFisier { get; set; }

        public string CaleFisier { get; set; }
        public int GroupId { get; set; }
        public TravelGroup Grup { get; set; }
    }
}
