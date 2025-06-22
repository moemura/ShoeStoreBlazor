using Microsoft.Extensions.Options;

namespace WebApp.Services.Payments;

public class VnPayPaymentGatewayService : IPaymentGatewayService
{
    private readonly VnPaySettings _settings;
    private readonly ILogger<VnPayPaymentGatewayService> _logger;

    public PaymentMethod SupportedMethod => PaymentMethod.VnPay;

    public VnPayPaymentGatewayService(
        IOptions<VnPaySettings> settings,
        ILogger<VnPayPaymentGatewayService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<PaymentResultDto> CreatePaymentUrl(PaymentRequestDto request)
    {
        try
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VnPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", _settings.Version);
            vnpay.AddRequestData("vnp_Command", _settings.Command);
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1"); // TODO: Get real IP from HttpContext
            vnpay.AddRequestData("vnp_Locale", _settings.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán cho đơn hàng: {request.OrderId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_TxnRef", tick);
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.PaymentBackReturnUrl);

            var paymentUrl = vnpay.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);

            _logger.LogInformation("VnPay payment URL created: {Url}", paymentUrl);

            return Task.FromResult(new PaymentResultDto
            {
                Success = true,
                PaymentUrl = paymentUrl,
                TransactionId = tick
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VnPay payment URL");
            return Task.FromResult(new PaymentResultDto
            {
                Success = false,
                ErrorMessage = "Internal error occurred"
            });
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

            var responseCode = callback.Parameters.GetValueOrDefault("vnp_ResponseCode", "");
            var isSuccess = responseCode == "00";

            var errorMessage = isSuccess ? null : GetVnPayErrorMessage(responseCode);

            return new PaymentResultDto
            {
                Success = isSuccess,
                TransactionId = callback.TransactionId,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying VnPay payment");
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
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in callback.Parameters)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }
            
            var vnp_SecureHash = callback.Parameters.GetValueOrDefault("vnp_SecureHash", "");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _settings.HashSecret);
            
            return Task.FromResult(checkSignature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating VnPay callback");
            return Task.FromResult(false);
        }
    }

    private static string GetVnPayErrorMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            "99" => "Các lỗi khác (lỗi còn lại, không có trong danh sách mã lỗi đã liệt kê)",
            _ => $"Giao dịch không thành công (Mã lỗi: {responseCode})"
        };
    }
} 