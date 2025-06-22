using WebApp.Services.Payments;

namespace WebApp.Services.Orders;

public interface IPaymentStrategy
{
    Task<PaymentProcessResult> ProcessPayment(OrderDto order);
}

public class PaymentProcessResult
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresRedirect => !string.IsNullOrEmpty(PaymentUrl);
}

public class CODPaymentStrategy : IPaymentStrategy
{
    public Task<PaymentProcessResult> ProcessPayment(OrderDto order)
    {
        // Thanh toán khi nhận hàng, luôn thành công
        return Task.FromResult(new PaymentProcessResult
        {
            Success = true
        });
    }
}

public class EWalletPaymentStrategy : IPaymentStrategy
{
    private readonly IPaymentService _paymentService;
    private readonly PaymentMethod _paymentMethod;

    public EWalletPaymentStrategy(IPaymentService paymentService, PaymentMethod paymentMethod)
    {
        _paymentService = paymentService;
        _paymentMethod = paymentMethod;
    }

    public async Task<PaymentProcessResult> ProcessPayment(OrderDto order)
    {
        try
        {
            var paymentRequest = new PaymentRequestDto
            {
                OrderId = order.Id,
                PaymentMethod = _paymentMethod,
                Amount = (decimal)order.TotalAmount
            };

            var result = await _paymentService.InitiatePayment(paymentRequest);

            return new PaymentProcessResult
            {
                Success = result.Success,
                PaymentUrl = result.PaymentUrl,
                TransactionId = result.TransactionId,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            return new PaymentProcessResult
            {
                Success = false,
                ErrorMessage = $"Payment initialization failed: {ex.Message}"
            };
        }
    }
}

public class PaymentStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentStrategy GetStrategy(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.COD => new CODPaymentStrategy(),
            PaymentMethod.MoMo => new EWalletPaymentStrategy(_serviceProvider.GetRequiredService<IPaymentService>(), PaymentMethod.MoMo),
            PaymentMethod.VnPay => new EWalletPaymentStrategy(_serviceProvider.GetRequiredService<IPaymentService>(), PaymentMethod.VnPay),
            PaymentMethod.ZaloPay => new EWalletPaymentStrategy(_serviceProvider.GetRequiredService<IPaymentService>(), PaymentMethod.ZaloPay),
            _ => new CODPaymentStrategy()
        };
    }
}
