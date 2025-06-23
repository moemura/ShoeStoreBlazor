namespace WebApp.Models.Entities;

public class VoucherUsage : BaseEntity<Guid>
{
    public string VoucherCode { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
    public double DiscountAmount { get; set; }
    public double OriginalAmount { get; set; }
    public double FinalAmount { get; set; }
    
    // Navigation properties
    public virtual Voucher Voucher { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual AppUser? User { get; set; }
} 