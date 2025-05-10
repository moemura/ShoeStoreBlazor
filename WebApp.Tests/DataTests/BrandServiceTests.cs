using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models.DTOs;
using Moq;
using WebApp.Data.Interfaces;
using WebApp.Data.Services;

namespace WebApp.Tests.DataTests
{
    public class BrandServiceTests
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly IBrandService _brandService;
        private readonly Mock<ICacheService> _mockCacheService;

        public BrandServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ShoeStoreDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            // Create a service provider for the DbContextFactory
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create the DbContextFactory with the required parameters
            _contextFactory = new DbContextFactory<ShoeStoreDbContext>(
                serviceProvider,
                options,
                new DbContextFactorySource<ShoeStoreDbContext>()
            );

            // Setup mock cache service
            _mockCacheService = new Mock<ICacheService>();
            _mockCacheService.Setup(x => x.GetOrSetAsync<IEnumerable<BrandDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<BrandDto>>>>(), It.IsAny<TimeSpan?>()))
                .Returns((string key, Func<Task<IEnumerable<BrandDto>>> factory, TimeSpan? expiration) => factory());
            _mockCacheService.Setup(x => x.GetOrSetAsync<BrandDto>(It.IsAny<string>(), It.IsAny<Func<Task<BrandDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns((string key, Func<Task<BrandDto>> factory, TimeSpan? expiration) => factory());

            _brandService = new BrandService(_contextFactory, _mockCacheService.Object);
        }

        [Fact]
        public async Task Create_ValidBrand_ReturnsBrandDto()
        {
            // Arrange
            var brandDto = new BrandDto
            {
                Name = "Test Brand",
                Description = "Test Description",
                Logo = "test-logo.png"
            };

            // Act
            var result = await _brandService.Create(brandDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(brandDto.Name, result.Name);
            Assert.Equal(brandDto.Description, result.Description);
            Assert.Equal(brandDto.Logo, result.Logo);
            Assert.NotNull(result.Id);
            Assert.NotEqual(default, result.CreatedAt);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetById_ExistingBrand_ReturnsBrandDto()
        {
            // Arrange
            var brandDto = new BrandDto
            {
                Name = "Test Brand",
                Description = "Test Description",
                Logo = "test-logo.png"
            };
            var created = await _brandService.Create(brandDto);

            // Act
            var result = await _brandService.GetById(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal(brandDto.Name, result.Name);

            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetOrSetAsync<BrandDto>(It.IsAny<string>(), It.IsAny<Func<Task<BrandDto>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetById_NonExistingBrand_ReturnsNull()
        {
            // Act
            var result = await _brandService.GetById("non-existing-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ExistingBrand_UpdatesSuccessfully()
        {
            // Arrange
            var brandDto = new BrandDto
            {
                Name = "Test Brand",
                Description = "Test Description",
                Logo = "test-logo.png"
            };
            var created = await _brandService.Create(brandDto);

            // Act
            created.Name = "Updated Brand";
            created.Description = "Updated Description";
            await _brandService.Update(created);

            // Assert
            var updated = await _brandService.GetById(created.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated Brand", updated.Name);
            Assert.Equal("Updated Description", updated.Description);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Delete_ExistingBrand_DeletesSuccessfully()
        {
            // Arrange
            var brandDto = new BrandDto
            {
                Name = "Test Brand",
                Description = "Test Description",
                Logo = "test-logo.png"
            };
            var created = await _brandService.Create(brandDto);

            // Act
            await _brandService.Delete(created.Id);

            // Assert
            var deleted = await _brandService.GetById(created.Id);
            Assert.Null(deleted);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Filter_ByName_ReturnsMatchingBrands()
        {
            // Arrange
            var brands = new[]
            {
                new BrandDto { Name = "Nike", Description = "Sport Brand" },
                new BrandDto { Name = "Adidas", Description = "Sport Brand" },
                new BrandDto { Name = "Puma", Description = "Sport Brand" }
            };

            foreach (var brand in brands)
            {
                await _brandService.Create(brand);
            }

            // Act
            var filter = new Dictionary<string, string> { { "name", "Ni" } };
            var result = await _brandService.Filter(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("Nike", result.First().Name);
        }

        [Fact]
        public async Task GetPagination_ReturnsCorrectPage()
        {
            // Arrange
            var brands = new[]
            {
                new BrandDto { Name = "Brand 1" },
                new BrandDto { Name = "Brand 2" },
                new BrandDto { Name = "Brand 3" },
                new BrandDto { Name = "Brand 4" },
                new BrandDto { Name = "Brand 5" }
            };

            foreach (var brand in brands)
            {
                await _brandService.Create(brand);
            }

            // Act
            var result = await _brandService.GetPagination(1, 2);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.ItemCount);
            Assert.Equal(3, result.PageCount);
            Assert.True(result.HasNext);
            Assert.False(result.HasPrevious);
        }

        [Fact]
        public async Task GetAll_ShouldUseCache()
        {
            // Arrange
            var cachedBrands = new List<BrandDto>
            {
                new BrandDto { Id = "1", Name = "Cached Brand 1" },
                new BrandDto { Id = "2", Name = "Cached Brand 2" }
            };
            _mockCacheService.Setup(x => x.GetOrSetAsync<IEnumerable<BrandDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<BrandDto>>>>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.FromResult<IEnumerable<BrandDto>>(cachedBrands));

            // Act
            var result = await _brandService.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Cached Brand 1", result.First().Name);
            Assert.Equal("Cached Brand 2", result.Last().Name);

            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetOrSetAsync<IEnumerable<BrandDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<BrandDto>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }
    }
} 