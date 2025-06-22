using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.VnPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model, string? url);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collection);
    }
}
