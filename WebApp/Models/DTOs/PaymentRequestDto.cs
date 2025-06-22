namespace WebApp.Models.DTOs;

public class PaymentRequestDto
{
    public Guid OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentTransactionDto? Transaction { get; set; }
}

public class PaymentCallbackDto
{
    public string? TransactionId { get; set; }
    public string? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string? Signature { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
} 