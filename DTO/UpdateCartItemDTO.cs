namespace RideWild.DTO
{
    public class UpdateCartItemDTO
    {
        public long CartItemId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
    }
}
