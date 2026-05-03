using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelNest.Models
{
    public class LocatieGrup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Locatie {  get; set; }
        public int GroupId { get; set; }
        public TravelGroup TravelGroup { get; set; }
        public string? HotelNume { get; set; }
        public string? HotelLink { get; set; }
        public DateOnly? CheckIn { get; set; }
        public DateOnly? CheckOut { get; set; }
    }
}
