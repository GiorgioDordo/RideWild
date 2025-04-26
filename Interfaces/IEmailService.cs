namespace RideWild.Interfaces
{
    public interface IEmailService
    {
        Task PswResetEmailAsync(string to, string subject, string emailContent);
    }
}
