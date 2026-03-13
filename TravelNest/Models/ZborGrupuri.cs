using System.ComponentModel.DataAnnotations;

namespace TravelNest.Models
{
    public class ZborGrupuri
    {
        [Key]
        public int Id { get; set; }
        public int GrupId { get; set; }
        public virtual TravelGroup Grup { get; set; } 
        public string NumeCompanie { get; set; }
        public string NumarZbor { get; set; }
        public string Logo { get; set; }
        public string OrasPlecare { get; set; }
        public string OrasSosire { get; set; }
        public string AeroportPlecare { get; set; }
        public string AeroportSosire { get; set; }
        public DateTime DataPlecare { get; set; }
        public DateTime DataSosire { get; set; }
        public decimal Pret { get; set; }
        public string? LinkZbor { get; set; }
    }
}
