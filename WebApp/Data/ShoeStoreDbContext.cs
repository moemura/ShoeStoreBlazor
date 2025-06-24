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
    public virtual DbSet<Promotion> Promotions { get; set; }
    public virtual DbSet<PromotionProduct> PromotionProducts { get; set; }
    public virtual DbSet<PromotionCategory> PromotionCategories { get; set; }
    public virtual DbSet<PromotionBrand> PromotionBrands { get; set; }

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

        // Promotion configuration
        builder.Entity<Promotion>()
            .HasKey(p => p.Id);
            
        builder.Entity<Promotion>()
            .Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Entity<Promotion>()
            .Property(p => p.Description)
            .HasMaxLength(500);
            
        builder.Entity<Promotion>()
            .HasIndex(p => new { p.StartDate, p.EndDate });
            
        builder.Entity<Promotion>()
            .HasIndex(p => p.IsActive);
            
        builder.Entity<Promotion>()
            .HasIndex(p => p.Priority);

        // PromotionProduct configuration
        builder.Entity<PromotionProduct>()
            .HasOne(pp => pp.Promotion)
            .WithMany(p => p.PromotionProducts)
            .HasForeignKey(pp => pp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionProduct>()
            .HasOne(pp => pp.Product)
            .WithMany()
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionProduct>()
            .HasIndex(pp => pp.ProductId);

        // PromotionCategory configuration
        builder.Entity<PromotionCategory>()
            .HasOne(pc => pc.Promotion)
            .WithMany(p => p.PromotionCategories)
            .HasForeignKey(pc => pc.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionCategory>()
            .HasOne(pc => pc.Category)
            .WithMany()
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionCategory>()
            .HasIndex(pc => pc.CategoryId);

        // PromotionBrand configuration
        builder.Entity<PromotionBrand>()
            .HasOne(pb => pb.Promotion)
            .WithMany(p => p.PromotionBrands)
            .HasForeignKey(pb => pb.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionBrand>()
            .HasOne(pb => pb.Brand)
            .WithMany()
            .HasForeignKey(pb => pb.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<PromotionBrand>()
            .HasIndex(pb => pb.BrandId);
    }
}
