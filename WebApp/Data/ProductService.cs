using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class ProductService(ShoeStoreDbContext dbContext) : IProductService
    {
        public async Task<IEnumerable<ProductDto>> GetAll()
        {
            var data = await dbContext.Products.ToListAsync();
            return data.Select(p => p.ToDto());
        }
        public async Task<ProductDto> GetById(string Id)
        {
            var product = await dbContext.Products.SingleOrDefaultAsync(p => p.Id == Id);
            return product.ToDto();
        }
        public async Task<ProductDto> Create(ProductDto dto)
        {
            if (dto == null)
                throw new Exception("Data must not null!");
            var product = dto.ToEntity();
            product.Id = Guid.CreateVersion7().ToString();
            product.CreatedAt = DateTime.UtcNow;
            await dbContext.Products.AddAsync(product);
            dto = product.ToDto();
            await dbContext.SaveChangesAsync();
            return dto;
        }
        public async Task Update(ProductDto dto)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new Exception("Product not found!");
            product.UpdatedAt = DateTime.UtcNow;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.SalePrice = dto.SalePrice;
            product.MainImage = dto.MainImage;
            product.Image = dto.Image;
            product.LikeCount = dto.LikeCount;

            dbContext.Update(product);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            var product = await dbContext.Products.FindAsync(id)
               ?? throw new Exception("Product not found!");
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }
        public async Task<PaginationData<ProductDto>> GetPagination(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0 || pageSize <= 0)
                throw new Exception("Invalid parameters");
            var products = await dbContext.Products.Take(pageSize).Skip(pageSize * (pageIndex - 1)).ToListAsync();
            return new PaginationData<ProductDto>
            {
                Data = products.Select(p => p.ToDto()).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                ItemCount = dbContext.Products.Count(),
                PageCount = dbContext.Products.Count()
            };
        }

        public async Task<IEnumerable<ProductDto>> Filter(Dictionary<string, string> filter)
        {
            throw new NotImplementedException();
        }
        public async Task<PaginationData<ProductDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter)
        {
            throw new NotImplementedException();
        }
    }
}
