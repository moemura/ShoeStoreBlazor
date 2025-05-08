using WebApp.Models;

namespace WebApp.Data
{
    public interface IProductService
    {
        Task<ProductDto> Create(ProductDto dto);
        Task Delete(string id);
        Task<IEnumerable<ProductDto>> Filter(Dictionary<string, string> filter);
        Task<PaginationData<ProductDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter);
        Task<IEnumerable<ProductDto>> GetAll();
        Task<ProductDto> GetById(string Id);
        Task<PaginationData<ProductDto>> GetPagination(int pageIndex, int pageSize);
        Task Update(ProductDto dto);
    }
}