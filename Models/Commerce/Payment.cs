namespace AirportApp.Models.Commerce;

public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public string? ApprovalUrl { get; set; }
    public string? CaptureId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Pendiente";
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmationDate { get; set; }
    public string? ResponseMessage { get; set; }
}
