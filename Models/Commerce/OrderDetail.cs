namespace AirportApp.Models.Commerce;

public class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int? FlightId { get; set; }
    public int? SeatId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
