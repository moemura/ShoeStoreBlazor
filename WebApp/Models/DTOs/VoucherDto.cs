namespace WebApp.Models.DTOs;

public class VoucherDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VoucherType Type { get; set; }
    public double Value { get; set; }
    public double? MinOrderAmount { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VoucherCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VoucherType Type { get; set; }
    public double Value { get; set; }
    public double? MinOrderAmount { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VoucherUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VoucherType Type { get; set; }
    public double Value { get; set; }
    public double? MinOrderAmount { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class VoucherValidationRequest
{
    public string Code { get; set; } = string.Empty;
    public double OrderAmount { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
}

public class VoucherValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public VoucherErrorCode? ErrorCode { get; set; }
    public VoucherDto? Voucher { get; set; }
    public double DiscountAmount { get; set; }
    public double FinalAmount { get; set; }
}

public class VoucherApplyRequest
{
    public string Code { get; set; } = string.Empty;
    public double OrderAmount { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
}

public class VoucherApplyResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public VoucherErrorCode? ErrorCode { get; set; }
    public double DiscountAmount { get; set; }
    public double FinalAmount { get; set; }
    public VoucherDto? Voucher { get; set; }
}

public class VoucherFilterRequest
{
    public string? SearchTerm { get; set; }
    public VoucherType? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
}

public class VoucherUsageDto
{
    public Guid Id { get; set; }
    public string VoucherCode { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
    public double DiscountAmount { get; set; }
    public double OriginalAmount { get; set; }
    public double FinalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VoucherStatisticsDto
{
    public string Code { get; set; } = string.Empty;
    public int TotalUsed { get; set; }
    public double TotalDiscountAmount { get; set; }
    public int UniqueUsers { get; set; }
    public int? UsageLimit { get; set; }
    public DateTime? FirstUsedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public enum VoucherErrorCode
{
    VoucherNotFound = 1,
    VoucherExpired = 2,
    VoucherNotStarted = 3,
    VoucherInactive = 4,
    UsageLimitExceeded = 5,
    OrderAmountTooLow = 6,
    UserAlreadyUsed = 7,
    VoucherAlreadyApplied = 8,
    InvalidVoucherCode = 9,
    SystemError = 10
} 