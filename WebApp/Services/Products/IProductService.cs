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
        Task<ProductDto> GetById(string Id);
        Task<PaginatedList<ProductDto>> GetPagination(int pageIndex, int pageSize);
        Task Update(ProductDto dto);
        Task Stock(string productId, IEnumerable<InventoryDto> inventories);
        Task<Inventory> CheckInventory(int inventoryId, int quantity);
    }
}