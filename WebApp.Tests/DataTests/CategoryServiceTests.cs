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
    public class CategoryServiceTests
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly ICategoryService _categoryService;
        private readonly Mock<ICacheService> _mockCacheService;

        public CategoryServiceTests()
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
            _mockCacheService.Setup(x => x.GetOrSetAsync<IEnumerable<CategoryDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<CategoryDto>>>>(), It.IsAny<TimeSpan?>()))
                .Returns((string key, Func<Task<IEnumerable<CategoryDto>>> factory, TimeSpan? expiration) => factory());
            _mockCacheService.Setup(x => x.GetOrSetAsync<CategoryDto>(It.IsAny<string>(), It.IsAny<Func<Task<CategoryDto>>>(), It.IsAny<TimeSpan?>()))
                .Returns((string key, Func<Task<CategoryDto>> factory, TimeSpan? expiration) => factory());

            _categoryService = new CategoryService(_contextFactory, _mockCacheService.Object);
        }

        [Fact]
        public async Task Create_ValidCategory_ReturnsCategoryDto()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Name = "Test Category",
                Description = "Test Description",
                Image = "test-image.png"
            };

            // Act
            var result = await _categoryService.Create(categoryDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryDto.Name, result.Name);
            Assert.Equal(categoryDto.Description, result.Description);
            Assert.Equal(categoryDto.Image, result.Image);
            Assert.NotNull(result.Id);
            Assert.NotEqual(default, result.CreatedAt);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetById_ExistingCategory_ReturnsCategoryDto()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Name = "Test Category",
                Description = "Test Description",
                Image = "test-image.png"
            };
            var created = await _categoryService.Create(categoryDto);

            // Act
            var result = await _categoryService.GetById(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
            Assert.Equal(categoryDto.Name, result.Name);

            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetOrSetAsync<CategoryDto>(It.IsAny<string>(), It.IsAny<Func<Task<CategoryDto>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetById_NonExistingCategory_ReturnsNull()
        {
            // Act
            var result = await _categoryService.GetById("non-existing-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ExistingCategory_UpdatesSuccessfully()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Name = "Test Category",
                Description = "Test Description",
                Image = "test-image.png"
            };
            var created = await _categoryService.Create(categoryDto);

            // Act
            created.Name = "Updated Category";
            created.Description = "Updated Description";
            await _categoryService.Update(created);

            // Assert
            var updated = await _categoryService.GetById(created.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated Category", updated.Name);
            Assert.Equal("Updated Description", updated.Description);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Delete_ExistingCategory_DeletesSuccessfully()
        {
            // Arrange
            var categoryDto = new CategoryDto
            {
                Name = "Test Category",
                Description = "Test Description",
                Image = "test-image.png"
            };
            var created = await _categoryService.Create(categoryDto);

            // Act
            await _categoryService.Delete(created.Id);

            // Assert
            var deleted = await _categoryService.GetById(created.Id);
            Assert.Null(deleted);

            // Verify cache was invalidated
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Filter_ByName_ReturnsMatchingCategories()
        {
            // Arrange
            var categories = new[]
            {
                new CategoryDto { Name = "Running Shoes", Description = "For running" },
                new CategoryDto { Name = "Casual Shoes", Description = "For casual wear" },
                new CategoryDto { Name = "Sports Shoes", Description = "For sports" }
            };

            foreach (var category in categories)
            {
                await _categoryService.Create(category);
            }

            // Act
            var filter = new Dictionary<string, string> { { "name", "Running" } };
            var result = await _categoryService.Filter(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal("Running Shoes", result.First().Name);
        }

        [Fact]
        public async Task GetPagination_ReturnsCorrectPage()
        {
            // Arrange
            var categories = new[]
            {
                new CategoryDto { Name = "Category 1" },
                new CategoryDto { Name = "Category 2" },
                new CategoryDto { Name = "Category 3" },
                new CategoryDto { Name = "Category 4" },
                new CategoryDto { Name = "Category 5" }
            };

            foreach (var category in categories)
            {
                await _categoryService.Create(category);
            }

            // Act
            var result = await _categoryService.GetPagination(1, 2);

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
            var cachedCategories = new List<CategoryDto>
            {
                new CategoryDto { Id = "1", Name = "Cached Category 1" },
                new CategoryDto { Id = "2", Name = "Cached Category 2" }
            };
            _mockCacheService.Setup(x => x.GetOrSetAsync<IEnumerable<CategoryDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<CategoryDto>>>>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.FromResult<IEnumerable<CategoryDto>>(cachedCategories));

            // Act
            var result = await _categoryService.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Cached Category 1", result.First().Name);
            Assert.Equal("Cached Category 2", result.Last().Name);

            // Verify cache was checked
            _mockCacheService.Verify(x => x.GetOrSetAsync<IEnumerable<CategoryDto>>(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<CategoryDto>>>>(), It.IsAny<TimeSpan?>()), Times.Once);
        }
    }
} 