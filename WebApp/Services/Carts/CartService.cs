using WebApp.Services.Catches;
using WebApp.Services.Products;
using WebApp.Services.Promotions;

namespace WebApp.Services.Carts;

public class CartService : ICartService
{
    private readonly ICacheService _cacheService;
    private readonly IProductService _productService;
    private readonly IPromotionService _promotionService;
    private const int CartExpireDays = 7;
    public CartService(ICacheService cacheService, IProductService productService, IPromotionService promotionService)
    {
        _cacheService = cacheService;
        _productService = productService;
        _promotionService = promotionService;
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
        var cart = await _cacheService.GetOrSetAsync(key, async () => new CartDto { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }, TimeSpan.FromDays(CartExpireDays));
        
        // Calculate total order amount for dynamic pricing
        var orderTotal = cart.Items.Sum(item => item.Price * item.Quantity);
        
        // Apply dynamic promotions to all cart items with order total validation
        foreach (var item in cart.Items)
        {
            await ApplyDynamicPromotionToCartItem(item, orderTotal);
        }
        
        return cart;
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
                ProductName = product?.Name ?? "",
                BrandName = product?.Brand?.Name ?? "",
                MainImage = product?.MainImage ?? "",
                Price = product?.Price ?? 0,
                SalePrice = product?.SalePrice, // Keep for backward compatibility, but will be superseded by PromotionPrice
                Size = inventory.SizeId
            };
            
            cart.Items.Add(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }
        
        // Recalculate promotions for entire cart after item changes
        var orderTotal = cart.Items.Sum(i => i.Price * i.Quantity);
        foreach (var cartItem in cart.Items)
        {
            await ApplyDynamicPromotionToCartItem(cartItem, orderTotal);
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
        
        // Recalculate promotions for remaining items
        var orderTotal = cart.Items.Sum(i => i.Price * i.Quantity);
        foreach (var cartItem in cart.Items)
        {
            await ApplyDynamicPromotionToCartItem(cartItem, orderTotal);
        }
        
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
        
        // Recalculate promotions for merged cart
        var orderTotal = userCart.Items.Sum(i => i.Price * i.Quantity);
        foreach (var cartItem in userCart.Items)
        {
            await ApplyDynamicPromotionToCartItem(cartItem, orderTotal);
        }
        
        userCart.UpdatedAt = DateTime.UtcNow;
        await _cacheService.GetOrSetAsync(userKey, async () => userCart, TimeSpan.FromDays(CartExpireDays));
        await _cacheService.RemoveAsync(guestKey);
    }

    private async Task ApplyPromotionToCartItem(CartItemDto item)
    {
        if (string.IsNullOrEmpty(item.ProductId))
            return;

        var promotionPrice = await _promotionService.CalculatePromotionPriceAsync(item.ProductId, item.Price);
        if (promotionPrice < item.Price)
        {
            item.PromotionPrice = promotionPrice;
            item.PromotionDiscount = item.Price - promotionPrice;
            item.HasActivePromotion = true;
            
            var bestPromotion = await _promotionService.GetBestPromotionForProductAsync(item.ProductId);
            item.PromotionName = bestPromotion?.Name;
        }
        else
        {
            // Reset promotion fields if no promotion applies
            item.PromotionPrice = null;
            item.PromotionDiscount = null;
            item.HasActivePromotion = false;
            item.PromotionName = null;
        }
    }

    private async Task ApplyDynamicPromotionToCartItem(CartItemDto item, double orderTotal)
    {
        if (string.IsNullOrEmpty(item.ProductId))
            return;

        var promotionPrice = await _promotionService.CalculatePromotionPriceWithOrderValidationAsync(
            item.ProductId, item.Price, orderTotal);
            
        if (promotionPrice < item.Price)
        {
            item.PromotionPrice = promotionPrice;
            item.PromotionDiscount = item.Price - promotionPrice;
            item.HasActivePromotion = true;
            
            var bestPromotion = await _promotionService.GetBestPromotionForProductWithOrderValidationAsync(
                item.ProductId, orderTotal);
            item.PromotionName = bestPromotion?.Name;
        }
        else
        {
            // Reset promotion fields if no promotion applies
            item.PromotionPrice = null;
            item.PromotionDiscount = null;
            item.HasActivePromotion = false;
            item.PromotionName = null;
        }
    }
}
