using WebApp.Models;

namespace WebApp.Services.Products
{
    public interface IProductService
    {
        Task<ProductDto> Create(ProductDto dto);
        Task Delete(string id);
        Task<IEnumerable<ProductDto>> Filter(Dictionary<string, string> filter);
        Task<PaginatedList<ProductDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter);
        Task<IEnumerable<ProductDto>> GetAll();
        Task<ProductDto> GetById(string id);
        Task<PaginatedList<ProductDto>> GetPagination(int pageIndex, int pageSize);
        Task Update(ProductDto dto);
        Task Stock(string productId, IEnumerable<InventoryDto> inventories);
        Task<Inventory> CheckInventory(int inventoryId, int quantity);
        Task RemoveProductCache();
        
        // New methods for dynamic pricing
        Task<ProductDto> GetByIdWithDynamicPricing(string id, double? orderTotal = null);
        Task<IEnumerable<ProductDto>> GetAllWithDynamicPricing(double? orderTotal = null);
        Task<PaginatedList<ProductDto>> GetPaginationWithDynamicPricing(int pageIndex, int pageSize, double? orderTotal = null);
        Task<PaginatedList<ProductDto>> FilterAndPaginWithDynamicPricing(int pageIndex, int pageSize, Dictionary<string, string> filter, double orderTotal);
    }
}