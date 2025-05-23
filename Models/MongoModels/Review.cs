using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace RideWild.Models.MongoModels
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Text { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        [Range(1,5)]
        public int Rating { get; set; } = 0;

        [BsonIgnoreIfNull]
        public bool? IsPositive { get; set; }
    }
}
