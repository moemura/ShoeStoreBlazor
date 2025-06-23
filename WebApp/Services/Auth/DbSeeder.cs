using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.Entities;

namespace WebApp.Services.Auth;

public class DbSeeder
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbSeeder> _logger;
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;

    public DbSeeder(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger<DbSeeder> logger,
        IDbContextFactory<ShoeStoreDbContext> dbContextFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Create roles if they don't exist
            await CreateRolesAsync();

            // Create admin user if it doesn't exist
            await CreateAdminUserAsync();

            // Seed sample vouchers
            await SeedVouchersAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task CreateRolesAsync()
    {
        var roles = new[] { "Admin", "Customer", "Staff" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation($"Created role: {role}");
            }
        }
    }

    private async Task CreateAdminUserAsync()
    {
        var adminEmail = _configuration["AdminUser:Email"] ?? "admin@shoestore.com";
        var adminPassword = _configuration["AdminUser:Password"] ?? "Admin@123";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumber = "0123456789",
                FullName = "System Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Created admin user successfully");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            // Ensure admin user has Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Added Admin role to existing admin user");
            }
        }
    }

    private async Task SeedVouchersAsync()
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // Check if vouchers already exist
        if (await dbContext.Vouchers.AnyAsync())
        {
            _logger.LogInformation("Vouchers already exist, skipping seed");
            return;
        }

        var vouchers = new List<Voucher>
        {
            new Voucher
            {
                Code = "WELCOME10",
                Name = "Chào mừng khách hàng mới",
                Description = "Giảm 10% cho đơn hàng đầu tiên từ 500,000 VND",
                Type = VoucherType.Percentage,
                Value = 10,
                MinOrderAmount = 500000,
                MaxDiscountAmount = 100000,
                UsageLimit = 1000,
                UsedCount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                Code = "SUMMER2024",
                Name = "Khuyến mãi hè 2024",
                Description = "Giảm 50,000 VND cho đơn hàng từ 1,000,000 VND",
                Type = VoucherType.FixedAmount,
                Value = 50000,
                MinOrderAmount = 1000000,
                MaxDiscountAmount = null,
                UsageLimit = 500,
                UsedCount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                Code = "FREESHIP",
                Name = "Miễn phí vận chuyển",
                Description = "Giảm 30,000 VND phí vận chuyển cho đơn từ 300,000 VND",
                Type = VoucherType.FixedAmount,
                Value = 30000,
                MinOrderAmount = 300000,
                MaxDiscountAmount = null,
                UsageLimit = null, // Không giới hạn
                UsedCount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Voucher
            {
                Code = "VIP20",
                Name = "Khách hàng VIP - Giảm 20%",
                Description = "Giảm 20% tối đa 200,000 VND cho khách hàng VIP",
                Type = VoucherType.Percentage,
                Value = 20,
                MinOrderAmount = 800000,
                MaxDiscountAmount = 200000,
                UsageLimit = 100,
                UsedCount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        dbContext.Vouchers.AddRange(vouchers);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation($"Seeded {vouchers.Count} sample vouchers");
    }
} 