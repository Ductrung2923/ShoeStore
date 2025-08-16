using System;
using System.Collections.Generic;

namespace PjPRN222.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
