using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Data;

public class ShoeStoreDbContext : IdentityDbContext<AppUser>
{
    public ShoeStoreDbContext(DbContextOptions<ShoeStoreDbContext> options) : base(options) { }
    public ShoeStoreDbContext() { }

    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Size> Sizes { get; set; }
    public virtual DbSet<Inventory> Inventories { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public virtual DbSet<Voucher> Vouchers { get; set; }
    public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Voucher configuration
        builder.Entity<Voucher>()
            .HasKey(v => v.Code);
        
        builder.Entity<Voucher>()
            .Property(v => v.Code)
            .HasMaxLength(50);
            
        builder.Entity<Voucher>()
            .Property(v => v.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Entity<Voucher>()
            .Property(v => v.Description)
            .HasMaxLength(500);
            
        builder.Entity<Voucher>()
            .HasIndex(v => v.StartDate);
            
        builder.Entity<Voucher>()
            .HasIndex(v => v.EndDate);
            
        builder.Entity<Voucher>()
            .HasIndex(v => v.IsActive);

        // VoucherUsage configuration
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Voucher)
            .WithMany(v => v.Usages)
            .HasForeignKey(vu => vu.VoucherCode)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Order)
            .WithMany()
            .HasForeignKey(vu => vu.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<VoucherUsage>()
            .HasOne(vu => vu.User)
            .WithMany()
            .HasForeignKey(vu => vu.UserId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.Entity<VoucherUsage>()
            .HasIndex(vu => new { vu.VoucherCode, vu.UserId });
            
        builder.Entity<VoucherUsage>()
            .HasIndex(vu => new { vu.VoucherCode, vu.GuestId });
    }
}
