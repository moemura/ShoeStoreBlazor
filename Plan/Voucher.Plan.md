# Kế hoạch thực hiện chức năng Apply Mã giảm giá (Voucher)

## 1. Phân tích Codebase hiện tại

### Cấu trúc đã có:
- ✅ **Order Entity**: Đã có trường `VoucherCode` (string nullable)
- ✅ **OrderCreateRequest**: Đã có trường `VoucherCode` 
- ✅ **VoucherDecorator**: Đã có pattern decorator cho voucher nhưng chưa implement logic
- ✅ **OrderTotalStrategyFactory**: Đã support voucher strategy
- ✅ **Database**: Đã có migration support VoucherCode trong Orders table

### Cần bổ sung:
- ❌ **Voucher Entity**: Chưa có entity quản lý voucher
- ❌ **Voucher Service**: Chưa có service validate và apply voucher
- ❌ **Voucher Controller**: Chưa có API endpoints cho voucher
- ❌ **Voucher Logic**: VoucherDecorator chưa có logic thực tế

## 2. Thiết kế Database Schema

### 2.1 Voucher Entity
```csharp
public class Voucher : BaseEntity<string> // Code làm primary key
{
    public string Code { get; set; } // SUMMER2024, WELCOME10
    public string Name { get; set; } // Tên voucher
    public string? Description { get; set; } // Mô tả
    public VoucherType Type { get; set; } // Percentage, FixedAmount
    public double Value { get; set; } // 10 (%), 50000 (VND)
    public double? MinOrderAmount { get; set; } // Đơn tối thiểu
    public double? MaxDiscountAmount { get; set; } // Giảm tối đa
    public int? UsageLimit { get; set; } // Số lượng sử dụng tối đa
    public int UsedCount { get; set; } = 0; // Đã sử dụng
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<VoucherUsage>? Usages { get; set; }
}

public enum VoucherType
{
    Percentage = 1, // Giảm theo %
    FixedAmount = 2 // Giảm số tiền cố định
}
```

### 2.2 VoucherUsage Entity (Track usage history)
```csharp
public class VoucherUsage : BaseEntity<Guid>
{
    public string VoucherCode { get; set; }
    public Guid OrderId { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
    public double DiscountAmount { get; set; }
    public double OriginalAmount { get; set; }
    public double FinalAmount { get; set; }
    
    // Navigation properties
    public virtual Voucher Voucher { get; set; }
    public virtual Order Order { get; set; }
    public virtual AppUser? User { get; set; }
}
```

## 3. Implementation Plan

### Phase 1: Core Infrastructure (Priority: HIGH)

#### 3.1 Database Migration
```bash
# Tạo migration mới
dotnet ef migrations add AddVoucherEntities
dotnet ef database update
```

#### 3.2 Update DbContext
```csharp
// WebApp/Data/ShoeStoreDbContext.cs
public virtual DbSet<Voucher> Vouchers { get; set; }
public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }

// Configure relationships trong OnModelCreating
```

#### 3.3 Create DTOs
```csharp
// WebApp/Models/DTOs/VoucherDto.cs
public class VoucherDto { ... }
public class VoucherValidationRequest { ... }
public class VoucherValidationResult { ... }
public class VoucherApplyResult { ... }
```

#### 3.4 Create Mapping Profiles
```csharp
// WebApp/Models/Mapping/VoucherMapping.cs
public static class VoucherMapping { ... }
```

### Phase 2: Business Logic (Priority: HIGH)

#### 3.5 Create Voucher Service
```csharp
// WebApp/Services/Vouchers/IVoucherService.cs
public interface IVoucherService
{
    Task<VoucherValidationResult> ValidateVoucherAsync(string code, double orderAmount, string? userId, string? guestId);
    Task<VoucherApplyResult> ApplyVoucherAsync(string code, double orderAmount, string? userId, string? guestId);
    Task<double> CalculateDiscountAsync(string code, double orderAmount);
    Task MarkVoucherUsedAsync(string code, Guid orderId, string? userId, string? guestId, double discountAmount, double originalAmount);
    
    // Admin functions
    Task<VoucherDto> CreateVoucherAsync(VoucherCreateRequest request);
    Task<VoucherDto> UpdateVoucherAsync(string code, VoucherUpdateRequest request);
    Task DeleteVoucherAsync(string code);
    Task<PaginatedList<VoucherDto>> GetVouchersAsync(int pageIndex, int pageSize, VoucherFilterRequest? filter = null);
}
```

#### 3.6 Implement VoucherService
```csharp
// WebApp/Services/Vouchers/VoucherService.cs
public class VoucherService : IVoucherService
{
    // Validation logic:
    // - Check voucher exists & active
    // - Check date range
    // - Check usage limit
    // - Check min order amount
    // - Check user hasn't used (nếu cần)
    
    // Apply logic:
    // - Calculate discount amount
    // - Apply max discount limit
    // - Return final amount
}
```

#### 3.7 Update VoucherDecorator
```csharp
// WebApp/Services/Orders/OrderTotalStrategies.cs
public class VoucherDecorator : IOrderTotalStrategy
{
    private readonly IVoucherService _voucherService;
    
    public async Task<double> CalculateTotal(OrderCreateRequest req)
    {
        var baseTotal = await _inner.CalculateTotal(req);
        
        if (string.IsNullOrEmpty(req.VoucherCode))
            return baseTotal;
            
        var applyResult = await _voucherService.ApplyVoucherAsync(
            req.VoucherCode, 
            baseTotal, 
            req.UserId, // Cần thêm vào OrderCreateRequest
            req.GuestId  // Cần thêm vào OrderCreateRequest
        );
        
        if (!applyResult.Success)
            throw new Exception(applyResult.ErrorMessage);
            
        return applyResult.FinalAmount;
    }
}
```

### Phase 3: API Layer (Priority: MEDIUM)

#### 3.8 Create Voucher Controller
```csharp
// WebApp/Controllers/VoucherController.cs
[ApiController]
[Route("api/[controller]")]
public class VoucherController : ControllerBase
{
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateVoucher([FromBody] VoucherValidationRequest request)
    
    [HttpPost("apply")]  
    [AllowAnonymous]
    public async Task<IActionResult> ApplyVoucher([FromBody] VoucherApplyRequest request)
    
    // Admin endpoints
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetVouchers(...)
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateVoucher(...)
    
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVoucher(...)
    
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVoucher(...)
}
```

#### 3.9 Update OrderService
```csharp
// WebApp/Services/Orders/OrderService.cs
public async Task<OrderCreationResult> CreateOrder(OrderCreateRequest req, string? userId, string? guestId)
{
    // ... existing logic ...
    
    // After order saved successfully, mark voucher as used
    if (!string.IsNullOrEmpty(req.VoucherCode))
    {
        await _voucherService.MarkVoucherUsedAsync(
            req.VoucherCode,
            order.Id,
            userId,
            guestId,
            originalTotal - finalTotal, // discount amount
            originalTotal
        );
    }
    
    // ... rest of logic ...
}
```

### Phase 4: Admin Interface (Priority: LOW)

#### 3.10 Create Blazor Admin Pages
```csharp
// WebApp/BlazorPages/Vouchers/VoucherPage.razor
// - CRUD operations
// - Usage statistics
// - Voucher management
```

### Phase 5: Frontend Integration (Priority: MEDIUM)

#### 3.11 Update React Frontend
```javascript
// shoestore-react/src/services/voucherService.js
export const voucherService = {
    validateVoucher: (code, orderAmount) => {...},
    applyVoucher: (code, orderAmount) => {...}
}

// shoestore-react/src/components/VoucherInput.jsx
// Component nhập mã voucher

// shoestore-react/src/pages/Checkout.jsx  
// Integrate voucher input vào checkout flow
```

## 4. Business Rules

### 4.1 Validation Rules
1. **Voucher tồn tại và active**
2. **Trong thời gian hiệu lực** (StartDate <= now <= EndDate)
3. **Chưa hết lượt sử dụng** (UsedCount < UsageLimit)
4. **Đơn hàng đủ điều kiện** (OrderAmount >= MinOrderAmount)
5. **User chưa sử dụng voucher này** (nếu set limit per user)

### 4.2 Calculation Rules
```csharp
// Percentage voucher
if (voucher.Type == VoucherType.Percentage)
{
    discount = orderAmount * (voucher.Value / 100);
    if (voucher.MaxDiscountAmount.HasValue)
        discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
}

// Fixed amount voucher  
else if (voucher.Type == VoucherType.FixedAmount)
{
    discount = Math.Min(voucher.Value, orderAmount);
}

finalAmount = orderAmount - discount;
```

### 4.3 Usage Tracking
- Lưu lại lịch sử sử dụng voucher
- Track user/guest đã sử dụng
- Tăng UsedCount khi apply thành công
- Rollback nếu order bị cancel

## 5. Error Handling

### 5.1 Validation Errors
```csharp
public enum VoucherErrorCode
{
    VoucherNotFound,
    VoucherExpired,
    VoucherNotStarted,
    VoucherInactive,
    UsageLimitExceeded,
    OrderAmountTooLow,
    UserAlreadyUsed,
    VoucherAlreadyApplied
}
```

### 5.2 Exception Messages
- Tiếng Việt cho user-facing messages
- Chi tiết technical info cho logging

## 6. Testing Strategy

### 6.1 Unit Tests
- VoucherService validation logic
- VoucherDecorator calculation logic
- Edge cases và error scenarios

### 6.2 Integration Tests
- Full voucher apply workflow
- Database transactions
- API endpoints

## 7. Performance Considerations

### 7.1 Caching Strategy
- Cache active vouchers (Redis/Memory)
- Cache user voucher usage
- Invalidate cache khi update voucher

### 7.2 Database Optimization
- Index trên VoucherCode
- Index trên StartDate, EndDate
- Composite index cho queries thường dùng

## 8. Security Considerations

### 8.1 Rate Limiting
- Limit voucher validation requests
- Prevent voucher brute force

### 8.2 Authorization
- Admin-only voucher management
- User chỉ được validate/apply voucher

## 9. Deployment Steps

### 9.1 Database Migration
```bash
# Backup database
# Run migration
# Verify data integrity
```

### 9.2 Service Registration
```csharp
// WebApp/ServiceContainer.cs
services.AddScoped<IVoucherService, VoucherService>();
```

### 9.3 Configuration
```json
// appsettings.json
{
  "VoucherSettings": {
    "MaxValidationAttempts": 5,
    "CacheExpiryMinutes": 30,
    "DefaultUsageLimit": 1000
  }
}
```

## 10. Future Enhancements

### 10.1 Advanced Features
- Multi-tier vouchers (VIP users)
- Product-specific vouchers
- Category-specific vouchers
- Auto-apply best voucher
- Voucher stacking rules

### 10.2 Analytics
- Voucher usage statistics
- ROI tracking
- User behavior analysis

## 11. Implementation Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Core Infrastructure | 2 days | None |
| Phase 2: Business Logic | 3 days | Phase 1 |
| Phase 3: API Layer | 2 days | Phase 2 |
| Phase 4: Admin Interface | 2 days | Phase 3 |
| Phase 5: Frontend Integration | 2 days | Phase 3 |
| Testing & Bug fixes | 2 days | All phases |

**Total Estimated Time: 13 days**

## 12. Risk Mitigation

### 12.1 Technical Risks
- **Database migration issues**: Test trên staging trước
- **Performance impact**: Monitor query performance
- **Concurrency issues**: Implement proper locking

### 12.2 Business Risks  
- **Voucher abuse**: Implement rate limiting và validation
- **Revenue impact**: Set appropriate limits và tracking
- **User experience**: Comprehensive testing

---

**Notes**: 
- Ưu tiên implement Phase 1-3 trước để có basic functionality
- Phase 4-5 có thể implement song song
- Cần coordinate với frontend team cho Phase 5
