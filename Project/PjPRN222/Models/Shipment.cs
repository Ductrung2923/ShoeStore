using System;
using System.Collections.Generic;

namespace PjPRN222.Models;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int OrderId { get; set; }

    public DateTime? ShippedDate { get; set; }

    public DateTime EstimatedDeliveryDate { get; set; }

    public string DeliveryStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
