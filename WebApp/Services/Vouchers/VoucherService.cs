using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.Entities;
using WebApp.Services.Catches;

namespace WebApp.Services.Vouchers;

public class VoucherService : IVoucherService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<VoucherService> _logger;
    private const string CACHE_KEY_PREFIX = "voucher:";
    private const string ACTIVE_VOUCHERS_CACHE_KEY = "vouchers:active";
    private const int CACHE_EXPIRY_MINUTES = 30;

    public VoucherService(
        IDbContextFactory<ShoeStoreDbContext> dbContextFactory,
        ICacheService cacheService,
        ILogger<VoucherService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _cacheService = cacheService;
        _logger = logger;
    }

    #region Validation & Application

    public async Task<VoucherValidationResult> ValidateVoucherAsync(string code, double orderAmount, string? userId, string? guestId)
    {
        try
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            // Get voucher from cache or database
            var voucher = await GetVoucherFromCacheOrDbAsync(code, dbContext);
            if (voucher == null)
            {
                return new VoucherValidationResult
                {
                    IsValid = false,
                    ErrorCode = VoucherErrorCode.VoucherNotFound,
                    ErrorMessage = "Mã giảm giá không tồn tại"
                };
            }

            // Validate voucher
            var validationResult = await ValidateVoucherRulesAsync(voucher, orderAmount, userId, guestId, dbContext);
            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            // Calculate discount
            var discountAmount = CalculateDiscount(voucher, orderAmount);
            var finalAmount = orderAmount - discountAmount;

            return new VoucherValidationResult
            {
                IsValid = true,
                Voucher = voucher.ToDto(),
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating voucher {Code}", code);
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.SystemError,
                ErrorMessage = "Lỗi hệ thống, vui lòng thử lại"
            };
        }
    }

    public async Task<VoucherApplyResult> ApplyVoucherAsync(string code, double orderAmount, string? userId, string? guestId)
    {
        var validationResult = await ValidateVoucherAsync(code, orderAmount, userId, guestId);
        
        return new VoucherApplyResult
        {
            Success = validationResult.IsValid,
            ErrorCode = validationResult.ErrorCode,
            ErrorMessage = validationResult.ErrorMessage,
            DiscountAmount = validationResult.DiscountAmount,
            FinalAmount = validationResult.FinalAmount,
            Voucher = validationResult.Voucher
        };
    }

    public async Task<double> CalculateDiscountAsync(string code, double orderAmount)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var voucher = await GetVoucherFromCacheOrDbAsync(code, dbContext);
        
        if (voucher == null || !voucher.IsActive)
            return 0;

        return CalculateDiscount(voucher, orderAmount);
    }

    public async Task MarkVoucherUsedAsync(string code, Guid orderId, string? userId, string? guestId, double discountAmount, double originalAmount)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var voucher = await dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
        if (voucher == null)
        {
            _logger.LogWarning("Attempted to mark non-existent voucher {Code} as used", code);
            return;
        }

        // Increment usage count
        voucher.UsedCount++;
        voucher.UpdatedAt = DateTime.UtcNow;

        // Create usage record
        var usage = VoucherMapping.CreateUsage(code, orderId, userId, guestId, discountAmount, originalAmount, originalAmount - discountAmount);
        dbContext.VoucherUsages.Add(usage);

        await dbContext.SaveChangesAsync();

        // Invalidate cache
        await InvalidateVoucherCacheAsync(code);
        
        _logger.LogInformation("Voucher {Code} marked as used by {UserId}/{GuestId} for order {OrderId}", 
            code, userId, guestId, orderId);
    }

    #endregion

    #region Admin CRUD Operations

    public async Task<VoucherDto> CreateVoucherAsync(VoucherCreateRequest request)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // Check if voucher code already exists
        var existingVoucher = await dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == request.Code.ToUpper().Trim());
        if (existingVoucher != null)
        {
            throw new InvalidOperationException($"Mã voucher '{request.Code}' đã tồn tại");
        }

        // Validate request
        ValidateVoucherRequest(request);

        var voucher = request.ToEntity();
        dbContext.Vouchers.Add(voucher);
        await dbContext.SaveChangesAsync();

        // Invalidate active vouchers cache
        await _cacheService.RemoveAsync(ACTIVE_VOUCHERS_CACHE_KEY);

        _logger.LogInformation("Created new voucher {Code}", voucher.Code);
        return voucher.ToDto();
    }

    public async Task<VoucherDto> UpdateVoucherAsync(string code, VoucherUpdateRequest request)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var voucher = await dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == code);
        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher '{code}' không tồn tại");
        }

        // Validate request
        ValidateVoucherUpdateRequest(request);

        voucher.UpdateFromRequest(request);
        await dbContext.SaveChangesAsync();

        // Invalidate cache
        await InvalidateVoucherCacheAsync(code);
        await _cacheService.RemoveAsync(ACTIVE_VOUCHERS_CACHE_KEY);

        _logger.LogInformation("Updated voucher {Code}", code);
        return voucher.ToDto();
    }

    public async Task DeleteVoucherAsync(string code)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var voucher = await dbContext.Vouchers
            .Include(v => v.Usages)
            .FirstOrDefaultAsync(v => v.Code == code);
            
        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher '{code}' không tồn tại");
        }

        // Check if voucher has been used
        if (voucher.UsedCount > 0)
        {
            throw new InvalidOperationException($"Không thể xóa voucher '{code}' đã được sử dụng. Hãy đặt trạng thái không hoạt động thay vì xóa.");
        }

        dbContext.Vouchers.Remove(voucher);
        await dbContext.SaveChangesAsync();

        // Invalidate cache
        await InvalidateVoucherCacheAsync(code);
        await _cacheService.RemoveAsync(ACTIVE_VOUCHERS_CACHE_KEY);

        _logger.LogInformation("Deleted voucher {Code}", code);
    }

    public async Task<VoucherDto?> GetVoucherByCodeAsync(string code)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var voucher = await GetVoucherFromCacheOrDbAsync(code, dbContext);
        return voucher?.ToDto();
    }

    public async Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageIndex, int pageSize, VoucherFilterRequest? filter = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var query = dbContext.Vouchers.AsQueryable();

        // Apply filters
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(v => 
                    v.Code.ToLower().Contains(searchTerm) || 
                    v.Name.ToLower().Contains(searchTerm) ||
                    (v.Description != null && v.Description.ToLower().Contains(searchTerm)));
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(v => v.Type == filter.Type.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(v => v.IsActive == filter.IsActive.Value);
            }

            if (filter.StartDateFrom.HasValue)
            {
                query = query.Where(v => v.StartDate >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                query = query.Where(v => v.StartDate <= filter.StartDateTo.Value);
            }

            if (filter.EndDateFrom.HasValue)
            {
                query = query.Where(v => v.EndDate >= filter.EndDateFrom.Value);
            }

            if (filter.EndDateTo.HasValue)
            {
                query = query.Where(v => v.EndDate <= filter.EndDateTo.Value);
            }
        }

        // Order by created date descending
        query = query.OrderByDescending(v => v.CreatedAt);

        var totalCount = await query.CountAsync();
        var vouchers = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var voucherDtos = vouchers.Select(v => v.ToDto()).ToList();

        return new PaginatedList<VoucherDto>(voucherDtos, pageIndex, pageSize, totalCount);
    }

    #endregion

    #region Usage Tracking & Statistics

    public async Task<PaginatedList<VoucherUsageDto>> GetVoucherUsagesAsync(string code, int pageIndex, int pageSize)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var query = dbContext.VoucherUsages
            .Where(vu => vu.VoucherCode == code)
            .OrderByDescending(vu => vu.CreatedAt);

        var totalCount = await query.CountAsync();
        var usages = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var usageDtos = usages.Select(u => u.ToDto()).ToList();

        return new PaginatedList<VoucherUsageDto>(usageDtos, pageIndex, pageSize, totalCount);
    }

    public async Task<IEnumerable<VoucherUsageDto>> GetUserVoucherUsagesAsync(string userId, int limit = 10)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var usages = await dbContext.VoucherUsages
            .Where(vu => vu.UserId == userId)
            .OrderByDescending(vu => vu.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return usages.Select(u => u.ToDto());
    }

    public async Task<VoucherStatisticsDto> GetVoucherStatisticsAsync(string code)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var voucher = await dbContext.Vouchers
            .Include(v => v.Usages)
            .FirstOrDefaultAsync(v => v.Code == code);

        if (voucher == null)
        {
            throw new InvalidOperationException($"Voucher '{code}' không tồn tại");
        }

        var usages = voucher.Usages?.ToList() ?? new List<VoucherUsage>();
        var totalDiscountAmount = usages.Sum(u => u.DiscountAmount);
        var uniqueUsers = usages.Where(u => !string.IsNullOrEmpty(u.UserId))
                                .Select(u => u.UserId)
                                .Distinct()
                                .Count();
        var firstUsedAt = usages.Any() ? usages.Min(u => u.CreatedAt) : (DateTime?)null;
        var lastUsedAt = usages.Any() ? usages.Max(u => u.CreatedAt) : (DateTime?)null;

        return new VoucherStatisticsDto
        {
            Code = voucher.Code,
            TotalUsed = voucher.UsedCount,
            TotalDiscountAmount = totalDiscountAmount,
            UniqueUsers = uniqueUsers,
            UsageLimit = voucher.UsageLimit,
            FirstUsedAt = firstUsedAt,
            LastUsedAt = lastUsedAt
        };
    }

    public async Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync()
    {
        var cacheKey = ACTIVE_VOUCHERS_CACHE_KEY;
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            
            var activeVouchers = await dbContext.Vouchers
                .Where(v => v.IsActive && v.StartDate <= now && v.EndDate >= now)
                .OrderBy(v => v.Name)
                .ToListAsync();

            return activeVouchers.Select(v => v.ToDto()).ToList();
        }, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));
    }

    public async Task<bool> CanUserUseVoucherAsync(string code, string? userId, string? guestId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var voucher = await GetVoucherFromCacheOrDbAsync(code, dbContext);
        if (voucher == null || !voucher.IsActive)
            return false;

        // Check if user has already used this voucher (if it's a one-time use voucher)
        var hasUsed = await dbContext.VoucherUsages
            .AnyAsync(vu => vu.VoucherCode == code && 
                           ((!string.IsNullOrEmpty(userId) && vu.UserId == userId) ||
                            (!string.IsNullOrEmpty(guestId) && vu.GuestId == guestId)));

        return !hasUsed;
    }

    #endregion

    #region Private Helper Methods

    private async Task<Voucher?> GetVoucherFromCacheOrDbAsync(string code, ShoeStoreDbContext dbContext)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{code.ToUpper()}";
        
        var cachedVoucher = await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var voucher = await dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == code.ToUpper());
            return voucher?.ToDto();
        }, TimeSpan.FromMinutes(CACHE_EXPIRY_MINUTES));
        
        if (cachedVoucher == null)
            return null;
            
        // Convert DTO back to entity for business logic
        return new Voucher
        {
            Code = cachedVoucher.Code,
            Name = cachedVoucher.Name,
            Description = cachedVoucher.Description,
            Type = cachedVoucher.Type,
            Value = cachedVoucher.Value,
            MinOrderAmount = cachedVoucher.MinOrderAmount,
            MaxDiscountAmount = cachedVoucher.MaxDiscountAmount,
            UsageLimit = cachedVoucher.UsageLimit,
            UsedCount = cachedVoucher.UsedCount,
            StartDate = cachedVoucher.StartDate,
            EndDate = cachedVoucher.EndDate,
            IsActive = cachedVoucher.IsActive,
            CreatedAt = cachedVoucher.CreatedAt,
            UpdatedAt = cachedVoucher.UpdatedAt
        };
    }

    private async Task<VoucherValidationResult> ValidateVoucherRulesAsync(Voucher voucher, double orderAmount, string? userId, string? guestId, ShoeStoreDbContext dbContext)
    {
        var now = DateTime.UtcNow;

        // Check if voucher is active
        if (!voucher.IsActive)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.VoucherInactive,
                ErrorMessage = "Mã giảm giá đã bị vô hiệu hóa"
            };
        }

        // Check date range
        if (now < voucher.StartDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.VoucherNotStarted,
                ErrorMessage = $"Mã giảm giá chưa có hiệu lực. Có hiệu lực từ {voucher.StartDate:dd/MM/yyyy}"
            };
        }

        if (now > voucher.EndDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.VoucherExpired,
                ErrorMessage = $"Mã giảm giá đã hết hạn vào {voucher.EndDate:dd/MM/yyyy}"
            };
        }

        // Check usage limit
        if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.UsageLimitExceeded,
                ErrorMessage = "Mã giảm giá đã hết lượt sử dụng"
            };
        }

        // Check minimum order amount
        if (voucher.MinOrderAmount.HasValue && orderAmount < voucher.MinOrderAmount.Value)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorCode = VoucherErrorCode.OrderAmountTooLow,
                ErrorMessage = $"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderAmount.Value:N0} VND"
            };
        }

        // Check if user has already used this voucher
        if (!string.IsNullOrEmpty(userId) || !string.IsNullOrEmpty(guestId))
        {
            var hasUsed = await dbContext.VoucherUsages
                .AnyAsync(vu => vu.VoucherCode == voucher.Code && 
                               ((!string.IsNullOrEmpty(userId) && vu.UserId == userId) ||
                                (!string.IsNullOrEmpty(guestId) && vu.GuestId == guestId)));

            if (hasUsed)
            {
                return new VoucherValidationResult
                {
                    IsValid = false,
                    ErrorCode = VoucherErrorCode.UserAlreadyUsed,
                    ErrorMessage = "Bạn đã sử dụng mã giảm giá này rồi"
                };
            }
        }

        return new VoucherValidationResult { IsValid = true };
    }

    private double CalculateDiscount(Voucher voucher, double orderAmount)
    {
        double discount = 0;

        if (voucher.Type == VoucherType.Percentage)
        {
            discount = orderAmount * (voucher.Value / 100);
            
            // Apply maximum discount limit if specified
            if (voucher.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
            }
        }
        else if (voucher.Type == VoucherType.FixedAmount)
        {
            discount = Math.Min(voucher.Value, orderAmount);
        }

        return Math.Round(discount, 0); // Round to nearest VND
    }

    private void ValidateVoucherRequest(VoucherCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Mã voucher không được để trống");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên voucher không được để trống");

        if (request.Value <= 0)
            throw new ArgumentException("Giá trị voucher phải lớn hơn 0");

        if (request.Type == VoucherType.Percentage && request.Value > 100)
            throw new ArgumentException("Giá trị voucher theo phần trăm không được vượt quá 100%");

        if (request.StartDate >= request.EndDate)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

        if (request.UsageLimit.HasValue && request.UsageLimit.Value <= 0)
            throw new ArgumentException("Giới hạn sử dụng phải lớn hơn 0");

        if (request.MinOrderAmount.HasValue && request.MinOrderAmount.Value < 0)
            throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm");

        if (request.MaxDiscountAmount.HasValue && request.MaxDiscountAmount.Value <= 0)
            throw new ArgumentException("Giá trị giảm tối đa phải lớn hơn 0");
    }

    private void ValidateVoucherUpdateRequest(VoucherUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Tên voucher không được để trống");

        if (request.Value <= 0)
            throw new ArgumentException("Giá trị voucher phải lớn hơn 0");

        if (request.Type == VoucherType.Percentage && request.Value > 100)
            throw new ArgumentException("Giá trị voucher theo phần trăm không được vượt quá 100%");

        if (request.StartDate >= request.EndDate)
            throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

        if (request.UsageLimit.HasValue && request.UsageLimit.Value <= 0)
            throw new ArgumentException("Giới hạn sử dụng phải lớn hơn 0");

        if (request.MinOrderAmount.HasValue && request.MinOrderAmount.Value < 0)
            throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm");

        if (request.MaxDiscountAmount.HasValue && request.MaxDiscountAmount.Value <= 0)
            throw new ArgumentException("Giá trị giảm tối đa phải lớn hơn 0");
    }

    private async Task InvalidateVoucherCacheAsync(string code)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{code.ToUpper()}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    #endregion
} 