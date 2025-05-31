using WebApp.Models;

namespace WebApp.Services.Brands
{
    public interface IBrandService
    {
        Task<BrandDto> Create(BrandDto dto);
        Task Delete(string id);
        Task<IEnumerable<BrandDto>> Filter(Dictionary<string, string> filter);
        Task<PaginatedList<BrandDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter);
        Task<IEnumerable<BrandDto>> GetAll();
        Task<BrandDto> GetById(string Id);
        Task<PaginatedList<BrandDto>> GetPagination(int pageIndex, int pageSize);
        Task Update(BrandDto dto);
    }
} 