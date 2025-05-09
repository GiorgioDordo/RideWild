
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideWild.DTO;
using RideWild.Models.AdventureModels;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public CartsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }


        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        {
            return await _context.Carts.ToListAsync();
        }


        // GET: api/Carts/5
        /*
         * recupero il carrello
         * converto l'oggetto in DTO
         * calcolo il prezzo totale
         */
        [HttpGet("{customerId}")]
        public async Task<ActionResult<Cart>> GetCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                return Ok(new
                {
                    Items = new List<CartItemDTO>(),
                    Total = 0
                });
            }

            var items = cart.CartItems.Select(ci => new CartItemDTO
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.ListPrice,
                TotalPrice = ci.Quantity * ci.Product.ListPrice
            }).ToList();

            var total = items.Sum(i => i.TotalPrice);

            return Ok(new 
            {
                Items = items,
                Total = total
            });
        }


        // PUT: api/Carts/5
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDTO updateCartItemDTO)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == updateCartItemDTO.CustomerId);

            if (cart == null)
                return NotFound("Carrello non trovato");

            var item = cart.CartItems
                .FirstOrDefault(ci => ci.Id == updateCartItemDTO.CartItemId);

            if (item == null)
                return NotFound("Prodotto non trovato");

            if (updateCartItemDTO.Quantity == 0)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                return Ok("Prodotto rimosso");
            }

            item.Quantity = updateCartItemDTO.Quantity;
            item.TotalPrice = updateCartItemDTO.Quantity * (int)item.Product.ListPrice;

            await _context.SaveChangesAsync();

            return Ok("Carrello aggiornato");
        }


        // POST: api/Carts/add
        /* 
        * recupero il carrello del cliente o lo crea se non esiste
        * aggiungo un prodotto al carrello
        * calcolo il totale della riga
        */
        [HttpPost("add")]
        public async Task<ActionResult<Cart>> AddCartItem(AddCartItemDTO addCartItemDTO)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == addCartItemDTO.CustomerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = addCartItemDTO.CustomerId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Products.FindAsync(addCartItemDTO.ProductId);
            if (product == null)
                return NotFound("Prodotto non trovato");

            var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == addCartItemDTO.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += addCartItemDTO.Quantity;
                existingItem.TotalPrice = existingItem.Quantity * (int)product.ListPrice;
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = addCartItemDTO.ProductId,
                    Quantity = addCartItemDTO.Quantity,
                    CartId = cart.Id,
                    TotalPrice = addCartItemDTO.Quantity * (int)product.ListPrice
                };

                cart.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCart), new{customerId = cart.CustomerId}, cart);
        }


        // DELETE: api/Carts/clear/5
        // elimino tutti i prodotti del carrello
        [HttpDelete("clear/{customerId}")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                return NotFound("Carrello non trovato");

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok("Carrello svuotato");
        }


        // DELETE: api/Carts/5
        // elimino un prodotto dal carrello
        [HttpDelete("remove/{CartItemId}")]
        public async Task<IActionResult> RemoveCartItem(long CartItemId)
        {
            var item = await _context.CartItems.FindAsync(CartItemId);
            if (item == null)
                return NotFound("Prodotto non trovato");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Prodotto rimosso");
        }
    }
}
