using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RideWild.Models
{
    public class CustomerData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Salt { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string AddressLine { get; set; }
    }
}
