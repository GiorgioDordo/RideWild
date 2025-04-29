using System;
using System.Collections.Generic;

namespace RideWild.DataModels;

public partial class CustomerData
{
    public long Id { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string AddressLine { get; set; } = null!;
}
