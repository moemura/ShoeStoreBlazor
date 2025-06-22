namespace WebApp.Services.Payments;

public interface IPaymentGatewayFactory
{
    IPaymentGatewayService GetPaymentGateway(PaymentMethod method);
}

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentGatewayFactory> _logger;

    public PaymentGatewayFactory(
        IServiceProvider serviceProvider,
        ILogger<PaymentGatewayFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IPaymentGatewayService GetPaymentGateway(PaymentMethod method)
    {
        try
        {
            return method switch
            {
                PaymentMethod.MoMo => _serviceProvider.GetRequiredService<MoMoPaymentGatewayService>(),
                PaymentMethod.VnPay => _serviceProvider.GetRequiredService<VnPayPaymentGatewayService>(),
                _ => throw new NotSupportedException($"Payment method {method} is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment gateway for method {PaymentMethod}", method);
            throw;
        }
    }
} 