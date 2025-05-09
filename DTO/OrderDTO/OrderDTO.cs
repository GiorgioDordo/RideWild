namespace RideWild.DTO.OrderDTO.OrderDTO
{
    public class OrderDTO
    {
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int BillToAddressId { get; set; } // addressId da associare al customer
        public int ShipToAddressId { get; set; } // addressId da associare al customer
        public string ShipMethod { get; set; } = null!;
        public string? Comment { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();

    }
}
