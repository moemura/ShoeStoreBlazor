using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Models.Mapping;
using WebApp.Models.DTOs;
using WebApp.Data.Interfaces;

namespace WebApp.Data.Services
{
    public class BrandService : IBrandService
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
        private readonly ICacheService _cacheService;
        private const string CACHE_PREFIX = "Brand_";

        public BrandService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory, ICacheService cacheService)
        {
            _dbContextFactory = dbContextFactory;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<BrandDto>> GetAll()
        {
            var cacheKey = $"{CACHE_PREFIX}All";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var data = await dbContext.Brands.ToListAsync();
                return data.Select(b => b.ToDto());
            });
        }

        public async Task<BrandDto> GetById(string Id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var brand = await dbContext.Brands.SingleOrDefaultAsync(b => b.Id == Id);
            return brand.ToDto();
        }

        public async Task<BrandDto> Create(BrandDto brand)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var entity = new Brand
            {
                Id = Guid.NewGuid().ToString(),
                Name = brand.Name,
                Description = brand.Description,
                Logo = brand.Logo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Brands.Add(entity);
            await dbContext.SaveChangesAsync();

            await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);

            return new BrandDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Logo = entity.Logo,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public async Task Update(BrandDto brand)
        {
            using var _context = await _dbContextFactory.CreateDbContextAsync();
            var entity = await _context.Brands.FindAsync(brand.Id);
            if (entity != null)
            {
                entity.Name = brand.Name;
                entity.Description = brand.Description;
                entity.Logo = brand.Logo;
                entity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
            }
        }

        public async Task Delete(string id)
        {
            using var _context = await _dbContextFactory.CreateDbContextAsync();
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();
                await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
            }
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

        public async Task<PaginationData<BrandDto>> GetPagination(int page, int pageSize)
        {
            var cacheKey = $"{CACHE_PREFIX}Page_{page}_Size_{pageSize}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var _context = await _dbContextFactory.CreateDbContextAsync();
                var query = _context.Brands.AsNoTracking();
                var totalItems = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BrandDto
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        Logo = b.Logo,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync();

                return new PaginationData<BrandDto>
                {
                    Data = items,
                    ItemCount = totalItems
                };
            });
        }
    }
} 