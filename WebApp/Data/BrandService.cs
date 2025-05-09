using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Models.Mapping;

namespace WebApp.Data
{
    public class BrandService : IBrandService
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;

        public BrandService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<BrandDto>> GetAll()
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var data = await dbContext.Brands.ToListAsync();
            return data.Select(b => b.ToDto());
        }

        public async Task<BrandDto> GetById(string Id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var brand = await dbContext.Brands.SingleOrDefaultAsync(b => b.Id == Id);
            return brand.ToDto();
        }

        public async Task<BrandDto> Create(BrandDto dto)
        {
            if (dto == null)
                throw new Exception("Data must not null!");
            
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var brand = dto.ToEntity();
            brand.Id = Guid.CreateVersion7().ToString();
            brand.CreatedAt = DateTime.UtcNow;
            await dbContext.Brands.AddAsync(brand);
            dto = brand.ToDto();
            await dbContext.SaveChangesAsync();
            return dto;
        }

        public async Task Update(BrandDto dto)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var brand = await dbContext.Brands.FirstOrDefaultAsync(b => b.Id == dto.Id)
                ?? throw new Exception("Brand not found!");
            brand.UpdatedAt = DateTime.UtcNow;
            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.Logo = dto.Logo;

            dbContext.Update(brand);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var brand = await dbContext.Brands.FirstOrDefaultAsync(b => b.Id == id)
                ?? throw new Exception("Brand not found!");
            dbContext.Brands.Remove(brand);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<BrandDto>> Filter(Dictionary<string, string> filter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var query = dbContext.Brands.AsQueryable();

            if (filter.ContainsKey("name") && !string.IsNullOrEmpty(filter["name"]))
            {
                query = query.Where(b => b.Name.Contains(filter["name"]));
            }

            var brands = await query.ToListAsync();
            return brands.Select(b => b.ToDto());
        }

        public async Task<PaginationData<BrandDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var query = dbContext.Brands.AsQueryable();

            if (filter.ContainsKey("name") && !string.IsNullOrEmpty(filter["name"]))
            {
                query = query.Where(b => b.Name.Contains(filter["name"]));
            }

            var totalItems = await query.CountAsync();
            var brands = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new PaginationData<BrandDto>
            {
                Data = brands.Select(b => b.ToDto()),
                PageIndex = pageIndex,
                PageSize = pageSize,
                ItemCount = totalItems,
                PageCount = pageCount,
                HasNext = pageIndex < pageCount,
                HasPrevious = pageIndex > 1
            };
        }

        public async Task<PaginationData<BrandDto>> GetPagination(int pageIndex, int pageSize)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var totalItems = await dbContext.Brands.CountAsync();
            var brands = await dbContext.Brands
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

            return new PaginationData<BrandDto>
            {
                Data = brands.Select(b => b.ToDto()),
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