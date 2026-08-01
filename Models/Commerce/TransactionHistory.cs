namespace AirportApp.Models.Commerce;

public class TransactionHistory
{
    public int TransactionHistoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? PaymentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
