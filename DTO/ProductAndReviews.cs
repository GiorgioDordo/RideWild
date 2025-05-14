using RideWild.Models.MongoModels;

namespace RideWild.DTO
{
    public class ProductAndReviews
    {
        public string Name { get; set; } = string.Empty;
        public List<Review> Reviews { get; set; } = [];
    }
}
