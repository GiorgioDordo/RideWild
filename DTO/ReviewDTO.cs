using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace RideWild.DTO
{
    public class ReviewDTO
    {
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedOn { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 0;
    }
}
