using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RideWild.DTO;
using RideWild.Models.MongoModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private IMongoCollection<Review> reviewsCollection;

        public ReviewController(IOptions<ReviewsDbConfig> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);
            reviewsCollection = database.GetCollection<Review>(options.Value.CollectionName);
        }

        //GET api/<ReviewController>/get
        [HttpGet("all")]
        public async Task<IEnumerable<Review>> GetAll()
        {
            return await reviewsCollection.Find(review => true).ToListAsync();
        }

        // GET api/<ReviewController>/5
        [HttpGet("{id}")]
        public async Task<IEnumerable<Review>> GetReviewsByProductId(int id)
        {
            return await reviewsCollection.Find(review => review.ProductId == id)
                .ToListAsync();
        }

        // POST api/Review/add
        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            if (review == null)
            {
                return BadRequest("Car cannot be null");
            }

            await reviewsCollection.InsertOneAsync(review);

            return CreatedAtAction(nameof(GetReviewsByProductId), new { id = review.Id }, review);
        }

        // PUT api/<ReviewController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(string id, [FromBody]ReviewDTO reviewDto)
        {
            var review = await reviewsCollection.Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound();
            }

            review.Title = reviewDto.Title;
            review.Text = reviewDto.Text;
            review.CreatedOn = DateTime.Now;
            review.Rating = reviewDto.Rating;

            await reviewsCollection.ReplaceOneAsync(r => r.Id == id, review);

            return NoContent();
        }

        // DELETE api/<ReviewController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            var review = await reviewsCollection.Find(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound();
            }

            await reviewsCollection.DeleteOneAsync(r => r.Id == id);
            
            return Ok("Review deleted");
        }
    }
}
