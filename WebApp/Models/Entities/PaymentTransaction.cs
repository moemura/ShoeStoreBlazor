namespace WebApp.Models.Entities;

public class PaymentTransaction : BaseEntity<Guid>
{
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; } // ID từ payment gateway
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PaymentTransactionStatus Status { get; set; }
    public string? PaymentGatewayResponse { get; set; } // JSON response from gateway
    public string? PaymentUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? FailureReason { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; }
}

public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Success = 3,
    Failed = 4,
    Cancelled = 5,
    Expired = 6,
    Refunded = 7
} 