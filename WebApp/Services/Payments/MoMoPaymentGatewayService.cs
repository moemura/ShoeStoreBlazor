using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace WebApp.Services.Payments;

public class MoMoPaymentGatewayService : IPaymentGatewayService
{
    private readonly MoMoSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MoMoPaymentGatewayService> _logger;

    public PaymentMethod SupportedMethod => PaymentMethod.MoMo;

    public MoMoPaymentGatewayService(
        IOptions<MoMoSettings> settings,
        HttpClient httpClient,
        ILogger<MoMoPaymentGatewayService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResultDto> CreatePaymentUrl(PaymentRequestDto request)
    {
        try
        {
            var accessKey = _settings.AccessKey;
            var amount = ((long)request.Amount).ToString();
            var orderId = request.OrderId.ToString();
            var orderInfo = $"Thanh toán đơn hàng #{orderId}";
            orderId = DateTime.Now.Ticks.ToString();
            var requestId = Guid.NewGuid().ToString();
            var returnUrl = _settings.ReturnUrl;
            var notifyUrl = _settings.NotifyUrl;
            var extraData = "";

            // Create raw signature theo format MoMo từ tài liệu
            var rawSignature = $"partnerCode={_settings.PartnerCode}" +
                             $"&accessKey={_settings.AccessKey}" +
                             $"&requestId={requestId}" +
                             $"&amount={amount}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo={orderInfo}" +
                             $"&returnUrl={returnUrl}" +
                             $"&notifyUrl={notifyUrl}" +
                             $"&extraData={extraData}";                            

            var signature = CreateSignature(rawSignature, _settings.SecretKey);

            var momoRequest = new
            {
                accessKey = accessKey,
                partnerCode = _settings.PartnerCode,
                requestType = _settings.RequestType,
                notifyUrl = notifyUrl,
                returnUrl = returnUrl,
                orderId = orderId,
                amount = amount,
                orderInfo = orderInfo,
                requestId = requestId,
                extraData = extraData,
                signature = signature,
            };

            var json = JsonSerializer.Serialize(momoRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending MoMo payment request: {Request}", json);

            var response = await _httpClient.PostAsync(_settings.MomoApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("MoMo payment response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MoMoResponse>(responseContent);

            if (momoResponse?.resultCode == 0 && !string.IsNullOrEmpty(momoResponse.payUrl))
            {
                return new PaymentResultDto
                {
                    Success = true,
                    PaymentUrl = momoResponse.payUrl,
                    TransactionId = requestId
                };
            }

            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = momoResponse?.message ?? "Unknown error from MoMo"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment URL");
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = "Internal error occurred"
            };
        }
    }

    public async Task<PaymentResultDto> VerifyPayment(PaymentCallbackDto callback)
    {
        try
        {
            if (!await ValidateCallback(callback))
            {
                return new PaymentResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid callback signature"
                };
            }

            var isSuccess = callback.Parameters.TryGetValue("resultCode", out var resultCode) && 
                           resultCode == "0";

            return new PaymentResultDto
            {
                Success = isSuccess,
                TransactionId = callback.TransactionId,
                ErrorMessage = isSuccess ? null : callback.Parameters.GetValueOrDefault("message", "Payment failed")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MoMo payment");
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = "Internal error occurred"
            };
        }
    }

    public Task<bool> ValidateCallback(PaymentCallbackDto callback)
    {
        try
        {
            var parameters = callback.Parameters;
            
            // Create signature string for validation
            var rawSignature = $"accessKey={_settings.AccessKey}" +
                             $"&amount={parameters.GetValueOrDefault("amount", "")}" +
                             $"&extraData={parameters.GetValueOrDefault("extraData", "")}" +
                             $"&message={parameters.GetValueOrDefault("message", "")}" +
                             $"&orderId={parameters.GetValueOrDefault("orderId", "")}" +
                             $"&orderInfo={parameters.GetValueOrDefault("orderInfo", "")}" +
                             $"&orderType={parameters.GetValueOrDefault("orderType", "")}" +
                             $"&partnerCode={_settings.PartnerCode}" +
                             $"&payType={parameters.GetValueOrDefault("payType", "")}" +
                             $"&requestId={parameters.GetValueOrDefault("requestId", "")}" +
                             $"&responseTime={parameters.GetValueOrDefault("responseTime", "")}" +
                             $"&resultCode={parameters.GetValueOrDefault("resultCode", "")}" +
                             $"&transId={parameters.GetValueOrDefault("transId", "")}";

            var signature = CreateSignature(rawSignature, _settings.SecretKey);

            return Task.FromResult(signature == callback.Signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MoMo callback");
            return Task.FromResult(false);
        }
    }

    private static string CreateSignature(string rawSignature, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(rawSignature);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private class MoMoResponse
    {
        public string? partnerCode { get; set; }
        public string? requestId { get; set; }
        public string? orderId { get; set; }
        public long amount { get; set; }
        public long responseTime { get; set; }
        public string? message { get; set; }
        public int resultCode { get; set; }
        public string? payUrl { get; set; }
        public string? shortLink { get; set; }
    }
} 