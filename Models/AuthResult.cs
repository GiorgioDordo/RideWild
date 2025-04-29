namespace RideWild.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

        public static AuthResult SuccessLogin(string token)
        {
            return new AuthResult { Success = true, Token = token };
        }
        public static AuthResult FailureLogin(string message)
        {
            return new AuthResult { Success = false, Message = message };
        }
    }

}
