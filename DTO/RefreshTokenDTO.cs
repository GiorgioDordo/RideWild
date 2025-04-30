using System.ComponentModel.DataAnnotations;

namespace RideWild.DTO
{
    public class RefreshTokenDTO
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
