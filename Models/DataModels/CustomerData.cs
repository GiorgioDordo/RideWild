using System;
using System.Collections.Generic;

namespace RideWild.Models.DataModels;

public partial class CustomerData
{
    public long Id { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastPasswordChange { get; set; }

    public bool IsMfaEnabled { get; set; } = false;

    public string? MfaCode { get; set; }

    public DateTime? MfaCodeExpiresAt { get; set; }
}
