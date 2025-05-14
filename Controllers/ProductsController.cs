using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RideWild.DTO;
using RideWild.Models.AdventureModels;
using RideWild.Models.MongoModels;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private IMongoCollection<Review> _reviewsCollection;

        public ProductsController(AdventureWorksLt2019Context context, IOptions<ReviewsDbConfig> options)
        {
            _context = context;

            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase(options.Value.DatabaseName);
            _reviewsCollection = database.GetCollection<Review>(options.Value.CollectionName);
        }
        
        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/ProductCatergory
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ProductCategory>>> GetProductCategory()
        //{
        //    return await _context.ProductCategory.Select(static c =>c.Name).ToListAsync();
        //}

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/search
        [HttpGet("search/{searchValue}")]
        public async Task<ActionResult<IEnumerable<ProductAndReviews>>> SearchProductByName(string searchValue)
        {
            // controllo che searchValue sia valido
            if(searchValue == "" || searchValue == null)
            {
                return BadRequest("Inserisci un valore di ricerca");
            }

            // cerca
            var products = await _context.Products.Where(p => p.Name.ToUpper().Contains(searchValue.ToUpper())).ToListAsync();

            if(products.Count == 0)
            {
                return NotFound("Nessun risultato di ricerca");
            }

            List<ProductAndReviews> productsList = [];

            // ciclo prodotti e aggiunta recensioni
            foreach(var product in products )
            {
                ProductAndReviews productAndReviews = new();

                productAndReviews.Name = product.Name;
                productAndReviews.Reviews = await _reviewsCollection.Find(review => review.ProductId == product.ProductId).SortByDescending(r => r.Rating).ToListAsync();

                productsList.Add(productAndReviews);
            }

            return Ok(productsList);
        }



        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO)
        {
            var product = await _context.Products.FindAsync(id);

              if (product == null)
            {
                return NotFound();
            }

            product.Name = productDTO.Name;
            product.ProductNumber = productDTO.ProductNumber;
            product.Color = productDTO.Color;
            product.StandardCost = productDTO.StandardCost;
            product.ListPrice = productDTO.ListPrice;
            product.Size = productDTO.Size;
            product.Weight = productDTO.Weight;
            product.ProductCategoryId = productDTO.ProductCategoryId;
            product.ProductModelId = productDTO.ProductModelId;
            product.SellStartDate = productDTO.SellStartDate == default ? DateTime.UtcNow : productDTO.SellStartDate;
            product.SellEndDate = null; // Set to null if not provided
            product.ThumbNailPhoto = productDTO.ThumbNailPhoto;
            product.ThumbnailPhotoFileName = productDTO.ThumbnailPhotoFileName;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!ProductExists(id))
            {
                return NotFound();
            }

            return NoContent();

            //Product product = new Product
            //{
            //    Name = productDTO.Name,
            //    ProductNumber = productDTO.ProductNumber,
            //    Color = productDTO.Color,
            //    StandardCost = productDTO.StandardCost,
            //    ListPrice = productDTO.ListPrice,
            //    Size = productDTO.Size,
            //    Weight = productDTO.Weight,
            //    ProductCategoryId = productDTO.ProductCategoryId,
            //    ProductModelId = productDTO.ProductModelId,
            //    SellStartDate = productDTO.SellStartDate == default ? DateTime.UtcNow : productDTO.SellStartDate,
            //    SellEndDate = null, // Set to null if not provided,
            //    ThumbNailPhoto = productDTO.ThumbNailPhoto,
            //    ThumbnailPhotoFileName = productDTO.ThumbnailPhotoFileName,
            //};

            //_context.Products.Update(product);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product); ;
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Product product = new Product
            {
                Name = productDTO.Name,
                ProductNumber = productDTO.ProductNumber,
                Color = productDTO.Color,
                StandardCost = productDTO.StandardCost,
                ListPrice = productDTO.ListPrice,
                Size = productDTO.Size,
                Weight = productDTO.Weight,
                ProductCategoryId = productDTO.ProductCategoryId,
                ProductModelId = productDTO.ProductModelId,
                SellStartDate = productDTO.SellStartDate == default ? DateTime.UtcNow : productDTO.SellStartDate,
                SellEndDate = null, // Set to null if not provided,
                ThumbNailPhoto = productDTO.ThumbNailPhoto,
                ThumbnailPhotoFileName = productDTO.ThumbnailPhotoFileName,
            };

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
