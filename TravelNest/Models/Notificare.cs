using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class Notificare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? TitluNotificare { get; set; }
        public string? MesajNotificare { get; set; } 
        public string? TipNotificare { get; set; }
        public string? Link { get; set; }
        public bool EsteCitita { get; set; } = false;
        public DateTime DataTrimitere { get; set; } = DateTime.Now;
        public int destinatarId { get; set; }
        public Profil Destinatar { get; set; }
        public int expeditorId { get; set; }  
        public Profil Expeditor { get; set; }

    }
}
