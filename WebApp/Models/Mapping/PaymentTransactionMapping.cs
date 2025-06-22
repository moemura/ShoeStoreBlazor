namespace WebApp.Models.Mapping;

public static class PaymentTransactionMapping
{
    public static PaymentTransactionDto ToDto(this PaymentTransaction transaction)
    {
        return new PaymentTransactionDto
        {
            Id = transaction.Id,
            OrderId = transaction.OrderId,
            PaymentMethod = transaction.PaymentMethod,
            TransactionId = transaction.TransactionId,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Status = transaction.Status,
            PaymentGatewayResponse = transaction.PaymentGatewayResponse,
            PaymentUrl = transaction.PaymentUrl,
            ExpiresAt = transaction.ExpiresAt,
            FailureReason = transaction.FailureReason,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }

    public static PaymentTransaction ToEntity(this PaymentRequestDto request)
    {
        return new PaymentTransaction
        {
            OrderId = request.OrderId,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            Currency = "VND",
            Status = PaymentTransactionStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Default 15 minutes expiry
            CreatedAt = DateTime.UtcNow
        };
    }
} 