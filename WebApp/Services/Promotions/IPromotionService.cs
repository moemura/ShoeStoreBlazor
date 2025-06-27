namespace WebApp.Services.Promotions;

public interface IPromotionService
{
    Task<PromotionDto?> GetByIdAsync(string id);
    Task<IEnumerable<PromotionDto>> GetAllAsync();
    Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
    Task<IEnumerable<PromotionDto>> GetPromotionsForProductAsync(string productId);
    Task<IEnumerable<PromotionDto>> GetPromotionsForCategoryAsync(string categoryId);
    Task<IEnumerable<PromotionDto>> GetPromotionsForBrandAsync(string brandId);
    Task<double> CalculatePromotionPriceAsync(string productId, double originalPrice);
    Task<PromotionDto?> GetBestPromotionForProductAsync(string productId);
    
    // Order-level promotion validation
    Task<IEnumerable<PromotionDto>> GetValidPromotionsForOrderAsync(IEnumerable<string> productIds, double orderTotal);
    Task<double> CalculatePromotionPriceWithOrderValidationAsync(string productId, double originalPrice, double orderTotal);
    Task<PromotionDto?> GetBestPromotionForProductWithOrderValidationAsync(string productId, double orderTotal);
    
    // CRUD operations for management
    Task<PromotionDto> CreateAsync(CreatePromotionRequest request);
    Task<PromotionDto> UpdateAsync(string id, CreatePromotionRequest request);
    Task<bool> DeleteAsync(string id);
    Task<bool> ToggleActiveStatusAsync(string id, bool isActive);
    Task RemovePromotionCache();
    
    // Batch processing methods for better performance
    Task<Dictionary<string, double>> CalculatePromotionPricesForProductsAsync(IEnumerable<string> productIds, IEnumerable<double> originalPrices, double? orderTotal = null);
    Task<Dictionary<string, PromotionDto?>> GetBestPromotionsForProductsAsync(IEnumerable<string> productIds, double? orderTotal = null);
} 