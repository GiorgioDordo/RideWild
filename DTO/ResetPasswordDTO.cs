using System.ComponentModel.DataAnnotations;

namespace RideWild.DTO
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
