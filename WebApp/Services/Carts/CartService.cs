using WebApp.Services.Catches;
using WebApp.Services.Products;

namespace WebApp.Services.Carts;

public class CartService : ICartService
{
    private readonly ICacheService _cacheService;
    private readonly IProductService _productService;
    private const int CartExpireDays = 7;
    public CartService(ICacheService cacheService, IProductService productService)
    {
        _cacheService = cacheService;
        _productService = productService;
    }
    private string GetCartKey(string userIdOrGuestId, bool isGuest = false)
        => isGuest ? $"cart:guest:{userIdOrGuestId}" : $"cart:{userIdOrGuestId}";

    public async Task<CartDto> GetCart(string userIdOrGuestId)
    {
        if (string.IsNullOrWhiteSpace(userIdOrGuestId))
            return new CartDto()
            {
                Items = [],
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
        var key = userIdOrGuestId.StartsWith("guest_") ? GetCartKey(userIdOrGuestId, true) : GetCartKey(userIdOrGuestId);
        return await _cacheService.GetOrSetAsync(key, async () => new CartDto { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, TimeSpan.FromDays(CartExpireDays));
    }

    public async Task<CartItemDto> AddOrUpdateItem(string userIdOrGuestId, CartItemAddOrUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(userIdOrGuestId))
            throw new ArgumentException("userIdOrGuestId cannot be null or empty", nameof(userIdOrGuestId));
        var key = userIdOrGuestId.StartsWith("guest_") ? GetCartKey(userIdOrGuestId, true) : GetCartKey(userIdOrGuestId);
        var cart = await GetCart(userIdOrGuestId);
        // Kiểm tra tồn kho
        var inventory = await _productService.CheckInventory(request.InventoryId, request.Quantity);
        if (inventory == null)
            throw new Exception("Not enough stock");
        var item = cart.Items.FirstOrDefault(i => i.InventoryId == request.InventoryId);
        if (item == null)
        {
            // Lấy thông tin sản phẩm
            var product = inventory.Product;
            item = new CartItemDto
            {
                InventoryId = request.InventoryId,
                Quantity = request.Quantity,
                ProductId = product?.Id ?? string.Empty,
                ProductName = product?.Name ?? string.Empty,
                Size = inventory.SizeId,
                MainImage = product?.MainImage,
                BrandName = product?.Brand?.Name,
                Price = product?.Price ?? 0,
                SalePrice = product?.SalePrice
            };
            cart.Items.Add(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }
        cart.UpdatedAt = DateTime.UtcNow;
        await _cacheService.GetOrSetAsync(key, async () => cart, TimeSpan.FromDays(CartExpireDays));
        return item;
    }

    public async Task RemoveItem(string userIdOrGuestId, int inventoryId)
    {
        if (string.IsNullOrWhiteSpace(userIdOrGuestId))
            throw new ArgumentException("userIdOrGuestId cannot be null or empty", nameof(userIdOrGuestId));
        var key = userIdOrGuestId.StartsWith("guest_") ? GetCartKey(userIdOrGuestId, true) : GetCartKey(userIdOrGuestId);
        var cart = await GetCart(userIdOrGuestId);
        cart.Items.RemoveAll(i => i.InventoryId == inventoryId);
        cart.UpdatedAt = DateTime.UtcNow;
        await _cacheService.GetOrSetAsync(key, async () => cart, TimeSpan.FromDays(CartExpireDays));
    }

    public async Task ClearCart(string userIdOrGuestId)
    {
        if (string.IsNullOrWhiteSpace(userIdOrGuestId))
            throw new ArgumentException("userIdOrGuestId cannot be null or empty", nameof(userIdOrGuestId));
        var key = userIdOrGuestId.StartsWith("guest_") ? GetCartKey(userIdOrGuestId, true) : GetCartKey(userIdOrGuestId);
        await _cacheService.RemoveAsync(key);
    }

    public async Task<int> GetCartItemCount(string userIdOrGuestId)
    {
        if (string.IsNullOrWhiteSpace(userIdOrGuestId))
            throw new ArgumentException("userIdOrGuestId cannot be null or empty", nameof(userIdOrGuestId));
        var cart = await GetCart(userIdOrGuestId);
        return cart.Items.Sum(i => i.Quantity);
    }

    public async Task MergeGuestCartToUser(string guestId, string userId)
    {
        if (string.IsNullOrWhiteSpace(guestId))
            throw new ArgumentException("guestId cannot be null or empty", nameof(guestId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId cannot be null or empty", nameof(userId));
        var guestKey = GetCartKey(guestId, true);
        var userKey = GetCartKey(userId);
        var guestCart = await _cacheService.GetOrSetAsync(guestKey, async () => new CartDto { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, TimeSpan.FromDays(CartExpireDays));
        var userCart = await GetCart(userId);
        foreach (var item in guestCart.Items)
        {
            var existing = userCart.Items.FirstOrDefault(i => i.InventoryId == item.InventoryId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                userCart.Items.Add(item);
        }
        userCart.UpdatedAt = DateTime.UtcNow;
        await _cacheService.GetOrSetAsync(userKey, async () => userCart, TimeSpan.FromDays(CartExpireDays));
        await _cacheService.RemoveAsync(guestKey);
    }
}
