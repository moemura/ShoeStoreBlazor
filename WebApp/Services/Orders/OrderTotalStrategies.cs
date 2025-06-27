using Microsoft.EntityFrameworkCore;
using WebApp.Services.Vouchers;
using WebApp.Services.Promotions;

namespace WebApp.Services.Orders;

public interface IOrderTotalStrategy
{
    Task<double> CalculateTotal(OrderCreateRequest req);
}

public class BaseOrderTotalStrategy : IOrderTotalStrategy
{
    private readonly ShoeStoreDbContext _dbContext;
    public BaseOrderTotalStrategy(ShoeStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public virtual async Task<double> CalculateTotal(OrderCreateRequest req)
    {
        double total = 0;
        foreach (var item in req.Items)
        {
            var inventory = await _dbContext.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory == null) continue;
            
            // Use Price as base price, not SalePrice (SalePrice is deprecated in favor of dynamic promotions)
            double price = inventory.Product?.Price ?? 0;
            total += item.Quantity * price;
        }
        return total;
    }
}

public class VoucherDecorator : IOrderTotalStrategy
{
    private readonly IOrderTotalStrategy _inner;
    private readonly IVoucherService _voucherService;
    private readonly string? _userId;
    private readonly string? _guestId;

    public VoucherDecorator(IOrderTotalStrategy inner, IVoucherService voucherService, string? userId, string? guestId)
    {
        _inner = inner;
        _voucherService = voucherService;
        _userId = userId;
        _guestId = guestId;
    }

    public async Task<double> CalculateTotal(OrderCreateRequest req)
    {
        var baseTotal = await _inner.CalculateTotal(req);
        
        if (string.IsNullOrEmpty(req.VoucherCode))
            return baseTotal;
            
        try
        {
            var applyResult = await _voucherService.ApplyVoucherAsync(
                req.VoucherCode, 
                baseTotal, 
                _userId, 
                _guestId
            );
            
            if (!applyResult.Success)
            {
                throw new InvalidOperationException(applyResult.ErrorMessage ?? "Không thể áp dụng mã giảm giá");
            }
                
            return applyResult.FinalAmount;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Lỗi áp dụng voucher: {ex.Message}");
        }
    }
}

public class PromotionDecorator : IOrderTotalStrategy
{
    private readonly IOrderTotalStrategy _inner;
    private readonly IPromotionService _promotionService;
    private readonly ShoeStoreDbContext _dbContext;

    public PromotionDecorator(IOrderTotalStrategy inner, IPromotionService promotionService, ShoeStoreDbContext dbContext)
    {
        _inner = inner;
        _promotionService = promotionService;
        _dbContext = dbContext;
    }

    public async Task<double> CalculateTotal(OrderCreateRequest req)
    {
        // First calculate the base total to determine if order meets minimum requirements
        double baseTotal = 0;
        var productPrices = new Dictionary<string, (double originalPrice, int quantity)>();
        
        foreach (var item in req.Items)
        {
            var inventory = await _dbContext.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory == null) continue;
            
            // Use Price as base price, not SalePrice (SalePrice is deprecated)
            double originalPrice = inventory.Product?.Price ?? 0;
            baseTotal += item.Quantity * originalPrice;
            
            if (inventory.Product?.Id != null)
            {
                productPrices[inventory.Product.Id] = (originalPrice, item.Quantity);
            }
        }
        
        // Now calculate with promotions that meet the minimum order amount requirement
        double totalWithPromotions = 0;
        
        foreach (var item in req.Items)
        {
            var inventory = await _dbContext.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory == null) continue;
            
            if (inventory.Product?.Id != null && productPrices.ContainsKey(inventory.Product.Id))
            {
                var (originalPrice, quantity) = productPrices[inventory.Product.Id];
                
                // Use the new method that validates MinOrderAmount
                double promotionPrice = await _promotionService.CalculatePromotionPriceWithOrderValidationAsync(
                    inventory.Product.Id, 
                    originalPrice, 
                    baseTotal
                );
                
                totalWithPromotions += quantity * promotionPrice;
            }
        }
        
        return totalWithPromotions;
    }
}

public class OrderTotalStrategyFactory
{
    private readonly ShoeStoreDbContext _dbContext;
    private readonly IVoucherService _voucherService;
    private readonly IPromotionService _promotionService;
    
    public OrderTotalStrategyFactory(ShoeStoreDbContext dbContext, IVoucherService voucherService, IPromotionService promotionService)
    {
        _dbContext = dbContext;
        _voucherService = voucherService;
        _promotionService = promotionService;
    }
    
    public IOrderTotalStrategy CreateStrategy(OrderCreateRequest orderRequest, string? userId, string? guestId)
    {
        IOrderTotalStrategy strategy = new BaseOrderTotalStrategy(_dbContext);
        
        // Apply promotions first (product-level discounts)
        strategy = new PromotionDecorator(strategy, _promotionService, _dbContext);
        
        // Then apply voucher (order-level discounts)
        if (orderRequest.VoucherCode != null)
            strategy = new VoucherDecorator(strategy, _voucherService, userId, guestId);
            
        return strategy;
    }
}
