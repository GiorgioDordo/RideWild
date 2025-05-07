using System.Security.Claims;

namespace RideWild.Utility
{
    public static class Helper
    {
        public static bool TryGetUserId(ClaimsPrincipal user, out int userId)
        {
            userId = 0;
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId);
        }

    }
}
