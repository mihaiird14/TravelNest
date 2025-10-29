using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelNest.Data;
using Microsoft.AspNetCore.Http;

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

    }
}
