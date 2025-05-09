using WebApp.Models;

namespace WebApp.Data
{
    public interface ICategoryService
    {
        Task<CategoryDto> Create(CategoryDto dto);
        Task Delete(string id);
        Task<IEnumerable<CategoryDto>> Filter(Dictionary<string, string> filter);
        Task<PaginationData<CategoryDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter);
        Task<IEnumerable<CategoryDto>> GetAll();
        Task<CategoryDto> GetById(string Id);
        Task<PaginationData<CategoryDto>> GetPagination(int pageIndex, int pageSize);
        Task Update(CategoryDto dto);
    }
} 