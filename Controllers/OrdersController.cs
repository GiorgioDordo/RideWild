using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RideWild.DTO.OrderDTO;
using RideWild.DTO.OrderDTO.OrderDTO;
using RideWild.Models.AdventureModels;
using RideWild.Models.DataModels;
using RideWild.Utility;
using System.Data.Common;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public OrdersController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }


        // mostra lista ordini CON PAGINAZIONE
        // USE: ADMIN DASHBOARD
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int page = 1, int pageSize = 20)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            try
            {
                var orders = await _context.SalesOrderHeaders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // mostra gli ordini per customerId
        // USE: USER DASHBOARD O ADMIN DASHBOARD FILTER
        [Authorize]
        [HttpGet("customer/")]
        public async Task<IActionResult> GetOrdersByCustomer()
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            try
            {
                var orders = await _context.SalesOrderHeaders
                    .Where(o => o.CustomerId == userId)
                    //.Skip((page - 1) * pageSize)
                    //.Take(pageSize)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // mostra l'ordine nel dettaglio
        // USE: ADMIN DASHBOARD
        [Authorize]
        [HttpGet("{orderId}")]
        public async Task<ActionResult<SalesOrderHeader>> GetOrder(int orderId)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            try
            {
                var order = await _context.SalesOrderHeaders
                    .Include(o => o.SalesOrderDetails)
                        .ThenInclude(d => d.Product)
                    .Include(o => o.ShipToAddress)
                    .Include(o => o.BillToAddress)
                    .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

                if (order == null)
                {
                    return NotFound($"Ordine con ID {orderId} non trovato.");
                }

                return order;
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // crea ordine
        // USE: USER E ADMIN DASHBOARD
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<SalesOrderHeader>> CreateOrder(OrderDTO orderDto)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            try
            {
                // creo il sales order header dal dto
                var newOrder = new SalesOrderHeader
                {
                    CustomerId = userId,
                    ShipToAddressId = orderDto.ShipToAddressId, // se non è corretto genera eccezione
                    BillToAddressId = orderDto.BillToAddressId, // se non è corretto genera eccezione
                    OrderDate = orderDto.OrderDate,
                    DueDate = orderDto.OrderDate.AddDays(10), // 10 giorni dall'ordine, poi quando modifico status passa a 5
                    ShipMethod = orderDto.ShipMethod,
                    Comment = orderDto.Comment,
                    OnlineOrderFlag = true,
                    TaxAmt = 0,
                    Freight = 10m, // costo di spedizione fisso
                    CreditCardApprovalCode = "" // varchar(15) Approval code provided by the credit card company.
                };

                // aggiungo l'order details al salesorderheader
                foreach (var OrderDetailDto in orderDto.OrderDetails)
                {
                    newOrder.SalesOrderDetails.Add(new SalesOrderDetail
                    {
                        ProductId = OrderDetailDto.ProductId, // passo dal carrello
                        OrderQty = OrderDetailDto.OrderQty,
                        UnitPrice = OrderDetailDto.UnitPrice,
                        UnitPriceDiscount = OrderDetailDto.UnitPriceDiscount // sconto da appliare al prodotto
                    });
                }

                // Aggiungo l'ordine alla base di dati.
                _context.SalesOrderHeaders.Add(newOrder);

                // Salviamo le modifiche nel database in modo asincrono.
                await _context.SaveChangesAsync();

                // ora hai newOrder aggiornato
                await _context.Entry(newOrder).ReloadAsync();

                // modifica taxamt
                newOrder.TaxAmt = newOrder.SubTotal * 0.22m;

                // Salviamo le modifiche nel database in modo asincrono.
                await _context.SaveChangesAsync();

                // Restituiamo una risposta positiva con l'ID dell'ordine appena creato.
                return CreatedAtAction("GetOrder", new { orderId = newOrder.SalesOrderId }, newOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // modifica ordine
        // USE: ADMIN DASHBOARD
        [Authorize(Policy = "Admin")]
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, OrderDTO updateOrderDto)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            try
            {
                // cerca l'ordine tramite id
                var orderToUpdate = await _context.SalesOrderHeaders
                    .Include(o => o.SalesOrderDetails)
                    .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

                // controlla se esiste
                if (orderToUpdate == null)
                    return NotFound($"Ordine con ID {orderId} non trovato.");

                // aggiorna sales header
                orderToUpdate.OrderDate = updateOrderDto.OrderDate;
                orderToUpdate.ShipMethod = updateOrderDto.ShipMethod;
                orderToUpdate.Comment = updateOrderDto.Comment;
                orderToUpdate.ShipToAddressId = updateOrderDto.ShipToAddressId;
                orderToUpdate.BillToAddressId = updateOrderDto.BillToAddressId;
                orderToUpdate.DueDate = orderToUpdate.OrderDate.AddDays(10); // 10 giorni dall'ordine, poi quando modifico status passa a 5
                orderToUpdate.ShipMethod = updateOrderDto.ShipMethod;
                orderToUpdate.ModifiedDate = DateTime.UtcNow;
                //orderToUpdate.CreditCardApprovalCode = "" // varchar(15) Approval code provided by the credit card company.

                // reset subtotal
                orderToUpdate.SubTotal = 0;
                orderToUpdate.TaxAmt = 0;
                orderToUpdate.Freight = 10;

                // cancella i dettagli precedenti per sovrascrivere completamente
                _context.SalesOrderDetails.RemoveRange(orderToUpdate.SalesOrderDetails);

                // aggiunge i nuovi dettagli
                foreach (var updateOrderDetailDto in updateOrderDto.OrderDetails)
                {
                    orderToUpdate.SalesOrderDetails.Add(new SalesOrderDetail
                    {
                        ProductId = updateOrderDetailDto.ProductId,
                        OrderQty = updateOrderDetailDto.OrderQty,
                        UnitPrice = updateOrderDetailDto.UnitPrice,
                        UnitPriceDiscount = updateOrderDetailDto.UnitPriceDiscount,
                        ModifiedDate = DateTime.UtcNow
                    });
                }

                // Salviamo le modifiche nel database in modo asincrono.
                await _context.SaveChangesAsync();

                // ora hai orderToUpdate aggiornato 1
                await _context.Entry(orderToUpdate).ReloadAsync();

                // modifica taxamt
                orderToUpdate.TaxAmt = orderToUpdate.SubTotal * 0.22m;

                // Salviamo le modifiche nel database in modo asincrono.
                await _context.SaveChangesAsync();

                // Restituiamo una risposta positiva con l'ID dell'ordine appena creato.
                return CreatedAtAction("GetOrder", new { orderId = orderToUpdate.SalesOrderId }, orderToUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
         
        }


        // chiamata patch per spedizione partita
        [Authorize(Policy = "Admin")]
        [HttpPatch("status")]
        public async Task<ActionResult<SalesOrderHeader>> PatchOrderStatus([FromBody] UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            try
            {
                var order = await _context.SalesOrderHeaders.FindAsync(updateOrderStatusDTO.OrderId);

                if (order == null)
                {
                    return NotFound($"Ordine con ID {updateOrderStatusDTO.OrderId} non trovato.");
                }

                switch (updateOrderStatusDTO.Status)
                {
                    case 1: // 1 = In process
                        order.Status = 1;
                        break;
                    case 2: // 2 = Approved
                        order.Status = 2;
                        break;
                    case 3: // 3 = Backordered
                        order.Status = 3;
                        break;
                    case 4: // 4 = Rejected
                        order.Status = 4;
                        order.Comment = "Ordine rifiutato, contattare il cliente";
                        break;
                    case 5: // 5 = Shipped
                        order.Status = 5;
                        order.ShipDate = DateTime.UtcNow;
                        order.DueDate = order.ShipDate.Value.AddDays(5); // calcola dueDate con shipDate + 5 giorni
                        break;
                    case 6: // 6 = Cancelled
                        order.Status = 6;
                        order.ShipDate = null;
                        order.Comment = "Ordine annullato";
                        break;
                    default:
                        return BadRequest("Status dev'essere un int compreso tra 1 e 6");
                }
                return CreatedAtAction("GetOrder", new { orderId = order.SalesOrderId }, order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }


        // cancella ordine 
        // USE: ADMIN DASHBOARD
        [Authorize(Policy = "Admin")]
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            var order = await _context.SalesOrderHeaders
                .Include(o => o.SalesOrderDetails)
                .FirstOrDefaultAsync(o => o.SalesOrderId == orderId);

            if (order == null)
                return NotFound();

            // rimuove i sales details
            _context.SalesOrderDetails.RemoveRange(order.SalesOrderDetails);

            // rimuove il sales header
            _context.SalesOrderHeaders.Remove(order);

            await _context.SaveChangesAsync();

            return Ok("");
        }
    }
}
