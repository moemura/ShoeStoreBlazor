using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models.DTOs;

namespace WebApp.Tests.DataTests
{
    public class CategoryServiceTests
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly ICategoryService _categoryService;

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
            _categoryService = new CategoryService(_contextFactory);
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
    }
} 