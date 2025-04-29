namespace RideWild.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }

        public static AuthResult SuccessAuth(string token, string refreshToken)
        {
            return new AuthResult { Success = true, Token = token, RefreshToken=refreshToken };
        }
        public static AuthResult FailureAuth(string message)
        {
            return new AuthResult { Success = false, Message = message };
        }
    }

}
