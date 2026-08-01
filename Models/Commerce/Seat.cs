namespace AirportApp.Models.Commerce;

public class Seat
{
    public int SeatId { get; set; }
    public int FlightId { get; set; }
    public string SeatNo { get; set; } = string.Empty;
    public string SeatClass { get; set; } = "Economy";
    public bool IsOccupied { get; set; }
    public decimal Price { get; set; }
}
