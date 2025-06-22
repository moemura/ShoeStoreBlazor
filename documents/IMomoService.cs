using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model,string action);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
