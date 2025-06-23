namespace WebApp.Models.Mapping;

public static class VoucherMapping
{
    public static VoucherDto ToDto(this Voucher voucher)
    {
        return new VoucherDto
        {
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            Type = voucher.Type,
            Value = voucher.Value,
            MinOrderAmount = voucher.MinOrderAmount,
            MaxDiscountAmount = voucher.MaxDiscountAmount,
            UsageLimit = voucher.UsageLimit,
            UsedCount = voucher.UsedCount,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            IsActive = voucher.IsActive,
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt
        };
    }

    public static Voucher ToEntity(this VoucherCreateRequest request)
    {
        return new Voucher
        {
            Code = request.Code.ToUpper().Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type,
            Value = request.Value,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateFromRequest(this Voucher voucher, VoucherUpdateRequest request)
    {
        voucher.Name = request.Name.Trim();
        voucher.Description = request.Description?.Trim();
        voucher.Type = request.Type;
        voucher.Value = request.Value;
        voucher.MinOrderAmount = request.MinOrderAmount;
        voucher.MaxDiscountAmount = request.MaxDiscountAmount;
        voucher.UsageLimit = request.UsageLimit;
        voucher.StartDate = request.StartDate;
        voucher.EndDate = request.EndDate;
        voucher.IsActive = request.IsActive;
        voucher.UpdatedAt = DateTime.UtcNow;
    }

    public static VoucherUsageDto ToDto(this VoucherUsage usage)
    {
        return new VoucherUsageDto
        {
            Id = usage.Id,
            VoucherCode = usage.VoucherCode,
            OrderId = usage.OrderId,
            UserId = usage.UserId,
            GuestId = usage.GuestId,
            DiscountAmount = usage.DiscountAmount,
            OriginalAmount = usage.OriginalAmount,
            FinalAmount = usage.FinalAmount,
            CreatedAt = usage.CreatedAt
        };
    }

    public static VoucherUsage ToEntity(this VoucherUsageDto dto)
    {
        return new VoucherUsage
        {
            Id = dto.Id,
            VoucherCode = dto.VoucherCode,
            OrderId = dto.OrderId,
            UserId = dto.UserId,
            GuestId = dto.GuestId,
            DiscountAmount = dto.DiscountAmount,
            OriginalAmount = dto.OriginalAmount,
            FinalAmount = dto.FinalAmount,
            CreatedAt = dto.CreatedAt
        };
    }

    public static VoucherUsage CreateUsage(string voucherCode, Guid orderId, string? userId, string? guestId, 
        double discountAmount, double originalAmount, double finalAmount)
    {
        return new VoucherUsage
        {
            Id = Guid.NewGuid(),
            VoucherCode = voucherCode,
            OrderId = orderId,
            UserId = userId,
            GuestId = guestId,
            DiscountAmount = discountAmount,
            OriginalAmount = originalAmount,
            FinalAmount = finalAmount,
            CreatedAt = DateTime.UtcNow
        };
    }
} 