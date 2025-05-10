using Microsoft.EntityFrameworkCore;
using WebApp.Data.Interfaces;

namespace WebApp.Data.Services
{
    public class SizeService : ISizeService
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly ICacheService _cacheService;

        public SizeService(IDbContextFactory<ShoeStoreDbContext> contextFactory, ICacheService cacheService)
        {
            _contextFactory = contextFactory;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Size>> GetAll()
        {
            return await _cacheService.GetOrSetAsync("sizes", async () =>
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var sizes = await context.Sizes
                    .OrderBy(s => s.Id)
                    .ToListAsync();
                return sizes;
            });
        }


        public async Task<Size> Create(Size size)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Sizes.AddAsync(size);
            await context.SaveChangesAsync();

            await _cacheService.RemoveByPrefixAsync("sizes");
            return size;
        }
    }
} 