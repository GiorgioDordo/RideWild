namespace RideWild.DTO
{
    public class RegisterDTO
    {

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public string? Phone { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
