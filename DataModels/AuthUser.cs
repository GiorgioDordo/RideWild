using System;
using System.Collections.Generic;

namespace RideWild.DataModels;

public partial class AuthUser
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string EmaiAddress { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;
}
