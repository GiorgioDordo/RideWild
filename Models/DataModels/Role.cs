using System;
using System.Collections.Generic;

namespace RideWild.Models.DataModels;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AuthUser> AuthUsers { get; set; } = new List<AuthUser>();
}
