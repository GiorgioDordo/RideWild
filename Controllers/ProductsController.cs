using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideWild.DTO;
using RideWild.Models.AdventureModels;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;


        public ProductsController(AdventureWorksLt2019Context context)
        {
            _context = context;
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

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO)
        //{
        //    Product product = new Product
        //    {
        //        Name = productDTO.Name,
        //        ProductNumber = productDTO.ProductNumber,
        //        Color = productDTO.Color,
        //        StandardCost = productDTO.StandardCost,
        //        ListPrice = productDTO.ListPrice,
        //        Size = productDTO.Size,
        //        Weight = productDTO.Weight,
        //        ProductCategoryId = productDTO.ProductCategoryId,
        //        ProductModelId = productDTO.ProductModelId,
        //        SellStartDate = productDTO.SellStartDate,
        //        SellEndDate = productDTO.SellEndDate
        //    };
        //    if (id != product.ProductId)
        //    {
        //        return BadRequest();
        //    }
        //    _context.Products.Update(product);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(PutProduct), new { }, productDTO);
        //}

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
