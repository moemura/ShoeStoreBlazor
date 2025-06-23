namespace WebApp.Services.Vouchers;

public interface IVoucherService
{
    // Validation & Application
    Task<VoucherValidationResult> ValidateVoucherAsync(string code, double orderAmount, string? userId, string? guestId);
    Task<VoucherApplyResult> ApplyVoucherAsync(string code, double orderAmount, string? userId, string? guestId);
    Task<double> CalculateDiscountAsync(string code, double orderAmount);
    Task MarkVoucherUsedAsync(string code, Guid orderId, string? userId, string? guestId, double discountAmount, double originalAmount);
    
    // Admin CRUD functions
    Task<VoucherDto> CreateVoucherAsync(VoucherCreateRequest request);
    Task<VoucherDto> UpdateVoucherAsync(string code, VoucherUpdateRequest request);
    Task DeleteVoucherAsync(string code);
    Task<VoucherDto?> GetVoucherByCodeAsync(string code);
    Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageIndex, int pageSize, VoucherFilterRequest? filter = null);
    
    // Usage tracking
    Task<PaginatedList<VoucherUsageDto>> GetVoucherUsagesAsync(string code, int pageIndex, int pageSize);
    Task<IEnumerable<VoucherUsageDto>> GetUserVoucherUsagesAsync(string userId, int limit = 10);
    
    // Statistics
    Task<VoucherStatisticsDto> GetVoucherStatisticsAsync(string code);
    Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync();
    Task<bool> CanUserUseVoucherAsync(string code, string? userId, string? guestId);
}

 