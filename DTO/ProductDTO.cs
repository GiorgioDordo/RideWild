using RideWild.Models.AdventureModels;
using System.ComponentModel.DataAnnotations;

namespace RideWild.DTO
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ProductNumber { get; set; }

        public string? Color { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StandardCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ListPrice { get; set; }

        public string? Size { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Weight { get; set; }

        public int? ProductCategoryId { get; set; }

        public int? ProductModelId { get; set; }

        [Required]
        [Range(typeof(DateTime), "1753/1/1", "9999/12/31", ErrorMessage = "SellStartDate must be between 1/1/1753 and 12/31/9999.")]
        public DateTime SellStartDate { get; set; }

        public byte[]? ThumbNailPhoto { get; set; }

        public string? ThumbnailPhotoFileName { get; set; }
    }
}
