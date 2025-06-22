namespace WebApp.Models;

public class PaymentSettings
{
    public int DefaultTimeoutMinutes { get; set; } = 15;
    public int MaxRetries { get; set; } = 3;
    public string[] EnabledGateways { get; set; } = [];
    public string BaseCallbackUrl { get; set; } = string.Empty;
}

public class MoMoSettings
{
    public string MomoApiUrl { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string PartnerCode { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; } = 15;
    public long MaxAmount { get; set; } = 50000000;
}

public class VnPaySettings
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CurrCode { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string PaymentBackReturnUrl { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; } = 15;
    public long MaxAmount { get; set; } = 50000000;
} 