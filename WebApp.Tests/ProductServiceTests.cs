using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Tests
{
    public class ProductServiceTests
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly IProductService _productService;

        public ProductServiceTests()
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

            _productService = new ProductService(_contextFactory);

            // Clear database before each test
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var products = new List<Product>
            {
                new Product { Id = "1", Name = "Test Product 1", Description ="For Test", Price = 100 },
                new Product { Id = "2", Name = "Test Product 2", Description ="For Test", Price = 200 },
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Act
            var result = await _productService.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "Test Product 1");
            Assert.Contains(result, p => p.Name == "Test Product 2");
        }

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var product = new Product { Id = "1", Name = "Test Product", Description = "For Test", Price = 100 };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await _productService.GetById("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(100, result.Price);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _productService.GetById("invalid-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_WithValidData_ShouldCreateProduct()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "New Product",
                Description = "Test Description",
                Price = 100,
                SalePrice = 80
            };

            // Act
            var result = await _productService.Create(productDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Product", result.Name);
            Assert.Equal(100, result.Price);
            Assert.Equal(80, result.SalePrice);
            Assert.NotNull(result.Id);
            Assert.NotEmpty(result.Id);

            // Verify in database
            using var context = _contextFactory.CreateDbContext();
            var createdProduct = await context.Products.FindAsync(result.Id);
            Assert.NotNull(createdProduct);
            Assert.Equal("New Product", createdProduct.Name);
        }

        [Fact]
        public async Task Update_WithValidData_ShouldUpdateProduct()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var product = new Product { Id = "1", Name = "Original Name", Description = "For Test", Price = 100 };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var updateDto = new ProductDto
            {
                Id = "1",
                Name = "Updated Name",
                Price = 200
            };

            // Act
            await _productService.Update(updateDto);

            // Assert
            using var verifyContext = _contextFactory.CreateDbContext();
            var updatedProduct = await verifyContext.Products.FindAsync("1");
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.Name);
            Assert.Equal(200, updatedProduct.Price);
        }

        [Fact]
        public async Task Delete_WithValidId_ShouldDeleteProduct()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var product = new Product { Id = "1", Name = "Test Product", Description ="For Test", Price = 100 };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            await _productService.Delete("1");

            // Assert
            using var verifyContext = _contextFactory.CreateDbContext();
            var deletedProduct = await verifyContext.Products.FindAsync("1");
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task Filter_WithName_ShouldReturnMatchingProducts()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var products = new List<Product>
            {
                new Product { Id = "1", Name = "Test Product 1", Description ="For Test", Price = 100 },
                new Product { Id = "2", Name = "Another Product", Description ="For Test", Price = 200 },
                new Product { Id = "3", Name = "Test Product 2", Description ="For Test", Price = 300 }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var filter = new Dictionary<string, string>
            {
                { "name", "Test" }
            };

            // Act
            var result = await _productService.Filter(filter);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => p.Name.Contains("Test"));
        }

        [Fact]
        public async Task Filter_WithPriceRange_ShouldReturnMatchingProducts()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var products = new List<Product>
            {
                new Product { Id = "1", Name = "Test Product 1", Description ="For Test", Price = 100 },
                new Product { Id = "2", Name = "Another Product", Description ="For Test", Price = 200 },
                new Product { Id = "3", Name = "Test Product 2", Description ="For Test", Price = 300 }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var filter = new Dictionary<string, string>
            {
                { "minPrice", "150" },
                { "maxPrice", "250" }
            };

            // Act
            var result = await _productService.Filter(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal(200, result.First().Price);
        }

        [Fact]
        public async Task GetPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var products = new List<Product>();
            for (int i = 1; i <= 10; i++)
            {
                products.Add(new Product { Id = i.ToString(), Name = $"Product {i}", Description = "For Test", Price = i * 100 });
            }
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Act
            var result = await _productService.GetPagination(2, 4);

            // Assert
            Assert.Equal(4, result.Data.Count());
            Assert.Equal(10, result.ItemCount);
            Assert.Equal(3, result.PageCount);
            Assert.True(result.HasNext);
            Assert.True(result.HasPrevious);
        }
    }
} 