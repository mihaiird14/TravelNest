using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TravelNest.Models
{
    public class ApplicationUser: IdentityUser
    {

        [Required]
        [StringLength(50)]
        [MinLength(5)]
        public string FirstName { set; get; }

        [Required]
        [StringLength(50)]
        [MinLength(5)]
        public string LastName { set; get; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Of Birth")]
        public DateTime DateOfBirth { get; set; }
    }
}
