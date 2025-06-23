namespace WebApp.Models.Entities;

public class Voucher : BaseEntity<string>
{
    public string Code { get; set; } = string.Empty; // SUMMER2024, WELCOME10
    public string Name { get; set; } = string.Empty; // Tên voucher
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