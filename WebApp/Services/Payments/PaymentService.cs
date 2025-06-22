using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.Mapping;

namespace WebApp.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly ShoeStoreDbContext _context;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ShoeStoreDbContext context,
        IPaymentGatewayFactory paymentGatewayFactory,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _paymentGatewayFactory = paymentGatewayFactory;
        _logger = logger;
    }

    public async Task<PaymentResultDto> InitiatePayment(PaymentRequestDto request)
    {
        try
        {
            _logger.LogInformation("Initiating payment for order {OrderId} with method {PaymentMethod} and amount {Amount}", 
                request.OrderId, request.PaymentMethod, request.Amount);

            // Kiểm tra order có tồn tại không
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for payment initiation", request.OrderId);
                return new PaymentResultDto
                {
                    Success = false,
                    ErrorMessage = "Order not found"
                };
            }

            // Kiểm tra payment method có được hỗ trợ không
            if (!IsEWalletPayment(request.PaymentMethod))
            {
                _logger.LogWarning("Unsupported payment method {PaymentMethod} for e-wallet payment", request.PaymentMethod);
                return new PaymentResultDto
                {
                    Success = false,
                    ErrorMessage = "Payment method not supported for e-wallet payment"
                };
            }

            // Tạo PaymentTransaction record
            var paymentTransaction = new PaymentTransaction
            {
                OrderId = request.OrderId,
                PaymentMethod = request.PaymentMethod,
                Amount = request.Amount,
                Currency = "VND",
                Status = PaymentTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15) // 15 phút hết hạn
            };

            _context.PaymentTransactions.Add(paymentTransaction);
            await _context.SaveChangesAsync();

                        // Tạo payment URL qua gateway
            _logger.LogInformation("Getting payment gateway for method {PaymentMethod}", request.PaymentMethod);
            var gateway = _paymentGatewayFactory.GetPaymentGateway(request.PaymentMethod);
            
            _logger.LogInformation("Creating payment URL via gateway for order {OrderId}", request.OrderId);
            var result = await gateway.CreatePaymentUrl(request);

            if (result.Success)
            {
                // Cập nhật PaymentTransaction với URL và TransactionId
                paymentTransaction.PaymentUrl = result.PaymentUrl;
                paymentTransaction.TransactionId = result.TransactionId;
                paymentTransaction.Status = PaymentTransactionStatus.Processing;
                paymentTransaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment initiated successfully for order {OrderId} with transaction {TransactionId} and URL {PaymentUrl}", 
                    request.OrderId, result.TransactionId, result.PaymentUrl);
            }
            else
            {
                // Cập nhật status thành Failed
                paymentTransaction.Status = PaymentTransactionStatus.Failed;
                paymentTransaction.FailureReason = result.ErrorMessage;
                paymentTransaction.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogError("Failed to initiate payment for order {OrderId}: {Error}", 
                    request.OrderId, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for order {OrderId}", request.OrderId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = "Internal error occurred"
            };
        }
    }

    public async Task<PaymentTransaction> ProcessPaymentCallback(PaymentCallbackDto callback)
    {
        try
        {
            // Tìm PaymentTransaction theo TransactionId
            var paymentTransaction = await _context.PaymentTransactions
                .Include(pt => pt.Order)
                .FirstOrDefaultAsync(pt => pt.TransactionId == callback.TransactionId);

            if (paymentTransaction == null)
            {
                _logger.LogWarning("Payment transaction not found for transaction ID: {TransactionId}",
                    callback.TransactionId);
                throw new InvalidOperationException("Payment transaction not found");
            }

            // Verify callback với gateway
            var gateway = _paymentGatewayFactory.GetPaymentGateway(paymentTransaction.PaymentMethod);
            var verificationResult = await gateway.VerifyPayment(callback);

            // Cập nhật PaymentTransaction
            paymentTransaction.Status = verificationResult.Success
                ? PaymentTransactionStatus.Success
                : PaymentTransactionStatus.Failed;

            paymentTransaction.PaymentGatewayResponse = System.Text.Json.JsonSerializer.Serialize(callback.Parameters);
            paymentTransaction.UpdatedAt = DateTime.UtcNow;

            if (!verificationResult.Success)
            {
                paymentTransaction.FailureReason = verificationResult.ErrorMessage;
            }

            // Cập nhật Order status nếu payment thành công
            if (verificationResult.Success && paymentTransaction.Order != null)
            {
                paymentTransaction.Order.Status = OrderStatus.Paid;
                paymentTransaction.Order.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Payment successful for order {OrderId}, transaction {TransactionId}",
                    paymentTransaction.OrderId, callback.TransactionId);
            }
            else if (!verificationResult.Success)
            {
                _logger.LogWarning("Payment failed for order {OrderId}, transaction {TransactionId}: {Error}",
                    paymentTransaction.OrderId, callback.TransactionId, verificationResult.ErrorMessage);
            }

            await _context.SaveChangesAsync();

            return paymentTransaction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback for transaction {TransactionId}",
                callback.TransactionId);
            throw;
        }
    }

    public async Task<PaymentTransaction?> GetPaymentTransaction(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Order)
            .FirstOrDefaultAsync(pt => pt.OrderId == orderId);
    }

    public async Task<PaymentTransaction?> GetPaymentTransactionByTransactionId(string transactionId)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Order)
            .FirstOrDefaultAsync(pt => pt.TransactionId == transactionId);
    }

    public async Task<bool> ExpirePaymentTransaction(Guid paymentTransactionId)
    {
        try
        {
            var paymentTransaction = await _context.PaymentTransactions.FindAsync(paymentTransactionId);
            if (paymentTransaction == null)
                return false;

            if (paymentTransaction.Status == PaymentTransactionStatus.Pending ||
                paymentTransaction.Status == PaymentTransactionStatus.Processing)
            {
                paymentTransaction.Status = PaymentTransactionStatus.Expired;
                paymentTransaction.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment transaction {PaymentTransactionId} expired", paymentTransactionId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring payment transaction {PaymentTransactionId}", paymentTransactionId);
            return false;
        }
    }

    private static bool IsEWalletPayment(PaymentMethod method)
    {
        return method is PaymentMethod.MoMo or PaymentMethod.VnPay or PaymentMethod.ZaloPay;
    }
} 