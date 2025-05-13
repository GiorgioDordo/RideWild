namespace RideWild.Models.MongoModels
{
    public class ReviewsDbConfig
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? CollectionName { get; set; }
        //public string? BsonDocumentsCollection { get; set; }
    }
}
