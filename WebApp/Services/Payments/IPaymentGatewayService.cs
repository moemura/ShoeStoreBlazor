namespace WebApp.Services.Payments;

public interface IPaymentGatewayService
{
    PaymentMethod SupportedMethod { get; }
    Task<PaymentResultDto> CreatePaymentUrl(PaymentRequestDto request);
    Task<PaymentResultDto> VerifyPayment(PaymentCallbackDto callback);
    Task<bool> ValidateCallback(PaymentCallbackDto callback);
} 