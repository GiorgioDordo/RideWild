using System;
using System.Collections.Generic;

namespace RideWild.Models;

public partial class CartItem
{
    public long Id { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int TotalPrice { get; set; }

    public int CartId { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
