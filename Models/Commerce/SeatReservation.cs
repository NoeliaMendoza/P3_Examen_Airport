namespace AirportApp.Models.Commerce;

public class SeatReservation
{
    public int SeatReservationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public int SeatId { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
    public bool IsConfirmed { get; set; }
}
