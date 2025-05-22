namespace RideWild.DTO
{
    public class CustomerDTO
    {
        public int? CustomerId { get; set; }
        public bool NameStyle { get; set; }

        public string? Title { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;

        public string? Suffix { get; set; }

        public string? CompanyName { get; set; }

        public string? SalesPerson { get; set; }


    }
}
