using Microsoft.EntityFrameworkCore;

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
            double price = inventory.Product?.SalePrice ?? inventory.Product?.Price ?? 0;
            total += item.Quantity * price;
        }
        return total;
    }
}

public class VoucherDecorator : IOrderTotalStrategy
{
    private readonly IOrderTotalStrategy _inner;
    private readonly ShoeStoreDbContext _dbContext;

    public VoucherDecorator(IOrderTotalStrategy inner, ShoeStoreDbContext dbContext)
    {
        _inner = inner;
        _dbContext = dbContext;
    }

    public async Task<double> CalculateTotal(OrderCreateRequest req)
    {
        var total = await _inner.CalculateTotal(req);
        return total;
    }
}

public class OrderTotalStrategyFactory
{
    private readonly ShoeStoreDbContext _dbContext;
    public OrderTotalStrategyFactory(ShoeStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IOrderTotalStrategy CreateStrategy(OrderCreateRequest orderRequest)
    {
        IOrderTotalStrategy strategy = new BaseOrderTotalStrategy(_dbContext);
        if (orderRequest.VoucherCode != null)
            strategy = new VoucherDecorator(strategy, _dbContext);
        return strategy;
    }
}
