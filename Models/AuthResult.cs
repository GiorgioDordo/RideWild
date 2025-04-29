namespace RideWild.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

        public static AuthResult SuccessAuth(string token)
        {
            return new AuthResult { Success = true, Token = token };
        }
        public static AuthResult FailureAuth(string message)
        {
            return new AuthResult { Success = false, Message = message };
        }
    }

}
