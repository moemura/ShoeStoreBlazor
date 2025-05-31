using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models.DTOs;
using Moq;
using WebApp.Services.Brands;
using WebApp.Services.Catches;

namespace WebApp.Tests.DataTests
{
    public class BrandServiceTests
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly IBrandService _brandService;

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
            _brandService = new BrandService(_contextFactory, new NoCacheService());
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
            var count = (await _brandService.GetAll()).Count();
            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.ItemCount);
            Assert.Equal(3, result.PageCount);
            Assert.True(result.HasNext);
            Assert.False(result.HasPrevious);
        }
    }
} 