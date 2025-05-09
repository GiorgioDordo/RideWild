namespace RideWild.DTO.OrderDTO.OrderDTO
{
    public class OrderDetailDTO
    {
        public int ProductId { get; set; }
        public short OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
    }
}
