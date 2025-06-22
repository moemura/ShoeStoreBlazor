
namespace WebApp.Services.Payments
{
    public interface IPaymentService
    {
        Task<bool> ExpirePaymentTransaction(Guid paymentTransactionId);
        Task<PaymentTransaction?> GetPaymentTransaction(Guid orderId);
        Task<PaymentTransaction?> GetPaymentTransactionByTransactionId(string transactionId);
        Task<PaymentResultDto> InitiatePayment(PaymentRequestDto request);
        Task<PaymentTransaction> ProcessPaymentCallback(PaymentCallbackDto callback);
    }
}