using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;
using WebApp.Models.Mapping;
using WebApp.Services.Products;
using WebApp.Services.Catches;

namespace WebApp.Tests.DataTests
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

            // Setup mock cache service


            // Clear database before each test
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            _productService = new ProductService(_contextFactory, new NoCacheService());
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
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(100, result.Price);
            Assert.Equal(80, result.SalePrice);
            Assert.NotNull(result.Id);
            Assert.NotEmpty(result.Id);

            // Verify in database  
            using var context = _contextFactory.CreateDbContext();
            var createdProduct = await context.Products.FindAsync(result.Id);
            Assert.NotNull(createdProduct);
            Assert.Equal("New Product", createdProduct.Name);
            Assert.Equal("Test Description", createdProduct.Description);
        }

        [Fact]
        public async Task Update_WithValidData_ShouldUpdateProduct()
        {
            // Arrange  
            using var context = _contextFactory.CreateDbContext();
            var product = new Product { Id = "1", Name = "Original Name", Description = "Original Description", Price = 100 };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var updateDto = new ProductDto
            {
                Id = "1",
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 200
            };

            // Act  
            await _productService.Update(updateDto);

            // Assert  
            using var verifyContext = _contextFactory.CreateDbContext();
            var updatedProduct = await verifyContext.Products.FindAsync("1");
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.Name);
            Assert.Equal("Updated Description", updatedProduct.Description);
            Assert.Equal(200, updatedProduct.Price);
        }

        [Fact]
        public async Task Delete_WithValidId_ShouldDeleteProduct()
        {
            // Arrange
            using var context = _contextFactory.CreateDbContext();
            var product = new Product { Id = "1", Name = "Test Product", Description = "For Test", Price = 100 };
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
        public async Task Create_WithValidCategoryAndBrand_ReturnsProductDto()
        {
            // Arrange  
            var categoryDto = new CategoryDto { Name = "Test Category" };
            var brandDto = new BrandDto { Name = "Test Brand" };

            using (var dbContext = await _contextFactory.CreateDbContextAsync())
            {
                var category = categoryDto.ToEntity();
                category.Id = Guid.CreateVersion7().ToString();
                var brand = brandDto.ToEntity();
                brand.Id = Guid.CreateVersion7().ToString();
                await dbContext.Categories.AddAsync(category);
                await dbContext.Brands.AddAsync(brand);
                await dbContext.SaveChangesAsync();
            }

            var productDto = new ProductDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 100,
                CategoryId = categoryDto.Id,
                BrandId = brandDto.Id
            };

            // Act  
            var result = await _productService.Create(productDto);

            // Assert  
            Assert.NotNull(result);
            Assert.Equal(productDto.Name, result.Name);
            Assert.Equal("Test Product Description", result.Description); // Assert description  
            Assert.Equal(categoryDto.Id, result.CategoryId);
            Assert.Equal(brandDto.Id, result.BrandId);
        }

        [Fact]
        public async Task Filter_ByCategoryAndBrand_ReturnsMatchingProducts()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Test Category" };
            categoryDto.Id = Guid.CreateVersion7().ToString();
            var brandDto = new BrandDto { Name = "Test Brand" };
            brandDto.Id = Guid.CreateVersion7().ToString();

            using (var dbContext = await _contextFactory.CreateDbContextAsync())
            {
                var category = categoryDto.ToEntity();
                var brand = brandDto.ToEntity();
                await dbContext.Categories.AddAsync(category);
                await dbContext.Brands.AddAsync(brand);
                await dbContext.SaveChangesAsync();
            }

            var products = new[]
            {
                new ProductDto
                {
                    Name = "Product 1",
                    Description = "Product 1 Description",
                    CategoryId = categoryDto.Id,
                    BrandId = brandDto.Id
                },
                new ProductDto
                {
                    Name = "Product 2",
                    Description = "Product 2 Description",
                    CategoryId = categoryDto.Id,
                    BrandId = brandDto.Id
                },
                new ProductDto
                {
                    Name = "Product 3",
                    Description = "Product 3 Description"
                }
            };

            foreach (var product in products)
            {
                await _productService.Create(product);
            }

            // Act
            var filter = new Dictionary<string, string>
            {
                { "categoryId", categoryDto.Id },
                { "brandId", brandDto.Id }
            };
            var result = await _productService.Filter(filter);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(categoryDto.Id, p.CategoryId));
            Assert.All(result, p => Assert.Equal(brandDto.Id, p.BrandId));
        }

        [Fact]
        public async Task GetById_IncludesCategoryAndBrand_ReturnsCompleteData()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Test Category" };
            categoryDto.Id = Guid.CreateVersion7().ToString();
            var brandDto = new BrandDto { Name = "Test Brand" };
            brandDto.Id = Guid.CreateVersion7().ToString();

            using (var dbContext = await _contextFactory.CreateDbContextAsync())
            {
                var category = categoryDto.ToEntity();
                var brand = brandDto.ToEntity();
                await dbContext.Categories.AddAsync(category);
                await dbContext.Brands.AddAsync(brand);
                await dbContext.SaveChangesAsync();
            }

            var productDto = new ProductDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 100,
                CategoryId = categoryDto.Id,
                BrandId = brandDto.Id
            };
            var created = await _productService.Create(productDto);

            // Act
            var result = await _productService.GetById(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryDto.Id, result.CategoryId);
            Assert.Equal(categoryDto.Name, result.CategoryName);
            Assert.Equal(brandDto.Id, result.BrandId);
            Assert.Equal(brandDto.Name, result.BrandName);
        }

        [Fact]
        public async Task Create_ValidProduct_ReturnsProductDto()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 100,
                SalePrice = 80,
                MainImage = "main.jpg",
                LikeCount = 0
            };

            // Act
            var result = await _productService.Create(productDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal("Test Product Description", result.Description);
            Assert.Equal(100, result.Price);
            Assert.Equal(80, result.SalePrice);
            Assert.NotNull(result.Id);
            Assert.NotEmpty(result.Id);
        }

        [Fact]
        public async Task GetById_ExistingProduct_ReturnsProductDto()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 100,
                SalePrice = 80,
                MainImage = "main.jpg",
                LikeCount = 0
            };
            var created = await _productService.Create(productDto);

            // Act
            var result = await _productService.GetById(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal("Test Product Description", result.Description);
            Assert.Equal(100, result.Price);
            Assert.Equal(80, result.SalePrice);
            Assert.Equal("main.jpg", result.MainImage);
            Assert.Equal(0, result.LikeCount);
        }

        [Fact]
        public async Task Update_ExistingProduct_UpdatesSuccessfully()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 100,
                SalePrice = 80,
                MainImage = "main.jpg",
                LikeCount = 0
            };
            var created = await _productService.Create(productDto);

            // Act
            created.Name = "Updated Product";
            created.Description = "Updated Product Description";
            await _productService.Update(created);

            // Assert
            using var verifyContext = _contextFactory.CreateDbContext();
            var updatedProduct = await verifyContext.Products.FindAsync(created.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Product", updatedProduct.Name);
            Assert.Equal("Updated Product Description", updatedProduct.Description);
            Assert.Equal(100, updatedProduct.Price);
            Assert.Equal(80, updatedProduct.SalePrice);
            Assert.Equal("main.jpg", updatedProduct.MainImage);
            Assert.Equal(0, updatedProduct.LikeCount);
        }

        [Fact]
        public async Task GetPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 20; i++)
            {
                var product = new ProductDto
                {
                    Name = $"Product {i}",
                    Description = $"Product {i} Description",
                    Price = i * 100
                };
                await _productService.Create(product);
            }
            // Act
            var result = await _productService.GetPagination(2, 5);

            // Assert
            Assert.Equal(5, result.PageSize);
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(5, result.Data?.Count());
            Assert.Equal(20, result.ItemCount);
            Assert.Equal(4, result.PageCount);
            Assert.True(result.HasNext);
            Assert.True(result.HasPrevious);
        }
    }
}