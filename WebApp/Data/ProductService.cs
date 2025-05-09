using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Models.Mapping;

namespace WebApp.Data
{
    public class ProductService : IProductService
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;

        public ProductService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<ProductDto>> GetAll()
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var data = await dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync();
            return data.Select(p => p.ToDto());
        }

        public async Task<ProductDto> GetById(string Id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .SingleOrDefaultAsync(p => p.Id == Id);
            return product.ToDto();
        }

        public async Task<ProductDto> Create(ProductDto dto)
        {
            if (dto == null)
                throw new Exception("Data must not null!");
            
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            // Validate Category and Brand if provided
            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    throw new Exception("Category not found!");
            }

            if (!string.IsNullOrEmpty(dto.BrandId))
            {
                var brandExists = await dbContext.Brands.AnyAsync(b => b.Id == dto.BrandId);
                if (!brandExists)
                    throw new Exception("Brand not found!");
            }

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
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new Exception("Product not found!");

            // Validate Category and Brand if provided
            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    throw new Exception("Category not found!");
            }

            if (!string.IsNullOrEmpty(dto.BrandId))
            {
                var brandExists = await dbContext.Brands.AnyAsync(b => b.Id == dto.BrandId);
                if (!brandExists)
                    throw new Exception("Brand not found!");
            }

            product.UpdatedAt = DateTime.UtcNow;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.SalePrice = dto.SalePrice;
            product.MainImage = dto.MainImage;
            product.Image = dto.Image;
            product.LikeCount = dto.LikeCount;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;

            dbContext.Update(product);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Product not found!");
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> Filter(Dictionary<string, string> filter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var query = dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (filter.ContainsKey("name") && !string.IsNullOrEmpty(filter["name"]))
            {
                query = query.Where(p => p.Name.Contains(filter["name"]));
            }

            if (filter.ContainsKey("minPrice") && double.TryParse(filter["minPrice"], out double minPrice))
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (filter.ContainsKey("maxPrice") && double.TryParse(filter["maxPrice"], out double maxPrice))
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            if (filter.ContainsKey("categoryId") && !string.IsNullOrEmpty(filter["categoryId"]))
            {
                query = query.Where(p => p.CategoryId == filter["categoryId"]);
            }

            if (filter.ContainsKey("brandId") && !string.IsNullOrEmpty(filter["brandId"]))
            {
                query = query.Where(p => p.BrandId == filter["brandId"]);
            }

            var products = await query.ToListAsync();
            return products.Select(p => p.ToDto());
        }

        public async Task<PaginationData<ProductDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var query = dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (filter.ContainsKey("name") && !string.IsNullOrEmpty(filter["name"]))
            {
                query = query.Where(p => p.Name.Contains(filter["name"]));
            }

            if (filter.ContainsKey("minPrice") && double.TryParse(filter["minPrice"], out double minPrice))
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (filter.ContainsKey("maxPrice") && double.TryParse(filter["maxPrice"], out double maxPrice))
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            if (filter.ContainsKey("categoryId") && !string.IsNullOrEmpty(filter["categoryId"]))
            {
                query = query.Where(p => p.CategoryId == filter["categoryId"]);
            }

            if (filter.ContainsKey("brandId") && !string.IsNullOrEmpty(filter["brandId"]))
            {
                query = query.Where(p => p.BrandId == filter["brandId"]);
            }

            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new PaginationData<ProductDto>
            {
                Data = products.Select(p => p.ToDto()),
                PageIndex = pageIndex,
                PageSize = pageSize,
                ItemCount = totalItems,
                PageCount = pageCount,
                HasNext = pageIndex < pageCount,
                HasPrevious = pageIndex > 1
            };
        }

        public async Task<PaginationData<ProductDto>> GetPagination(int pageIndex, int pageSize)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var totalItems = await dbContext.Products.CountAsync();
            var products = await dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new PaginationData<ProductDto>
            {
                Data = products.Select(p => p.ToDto()),
                PageIndex = pageIndex,
                PageSize = pageSize,
                ItemCount = totalItems,
                PageCount = pageCount,
                HasNext = pageIndex < pageCount,
                HasPrevious = pageIndex > 1
            };
        }
    }
}
