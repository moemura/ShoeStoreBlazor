using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using WebApp.Data;
using WebApp.Models.DTOs;
using WebApp.Models.Mapping;
using Xunit.Abstractions;

namespace WebApp.Tests.PerformanceTests
{
    public class ProductPerformanceTests : IDisposable
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _contextFactory;
        private readonly IProductService _productService;
        private readonly ITestOutputHelper _output;
        private const int LARGE_DATASET_SIZE = 10000;
        private const int MEDIUM_DATASET_SIZE = 1000;
        private const int SMALL_DATASET_SIZE = 100;
        private const int MAX_EXECUTION_TIME_MS = 1000; // 1 second

        public ProductPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            var options = new DbContextOptionsBuilder<ShoeStoreDbContext>()
                .UseInMemoryDatabase(databaseName: $"PerformanceTestDb_{Guid.NewGuid()}")
                .Options;

            _contextFactory = new DbContextFactory<ShoeStoreDbContext>(
              serviceProvider,
              options,
              new DbContextFactorySource<ShoeStoreDbContext>()
          );
            _productService = new ProductService(_contextFactory);
        }

        public void Dispose()
        {
            using var context = _contextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
        }

        private void ReportPerformance(string operationName, long elapsedMs, int itemCount = 0)
        {
            var averageTime = itemCount > 0 ? (double)elapsedMs / itemCount : elapsedMs;
            _output.WriteLine($"Performance Report - {operationName}:");
            _output.WriteLine($"Total Time: {elapsedMs}ms");
            if (itemCount > 0)
            {
                _output.WriteLine($"Items Processed: {itemCount}");
                _output.WriteLine($"Average Time per Item: {averageTime:F2}ms");
            }
            _output.WriteLine($"Status: {(elapsedMs < MAX_EXECUTION_TIME_MS ? "PASS" : "FAIL")}");
            _output.WriteLine("----------------------------------------");
        }

        [Fact]
        public async Task GetPagination_WithLargeDataset_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            await CreateLargeDataset(LARGE_DATASET_SIZE);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _productService.GetPagination(1, 100);
            stopwatch.Stop();

            // Assert
            ReportPerformance("Pagination", stopwatch.ElapsedMilliseconds, result.Data.Count());
            Assert.True(stopwatch.ElapsedMilliseconds < MAX_EXECUTION_TIME_MS,
                $"Pagination took {stopwatch.ElapsedMilliseconds}ms, expected less than {MAX_EXECUTION_TIME_MS}ms");
            Assert.Equal(100, result.Data.Count());
        }

        [Fact]
        public async Task Filter_WithComplexQuery_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            await CreateLargeDataset(LARGE_DATASET_SIZE);
            var filter = new Dictionary<string, string>
            {
                { "name", "Test" },
                { "minPrice", "100" },
                { "maxPrice", "1000" }
            };
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _productService.Filter(filter);
            stopwatch.Stop();

            // Assert
            ReportPerformance("Complex Filter", stopwatch.ElapsedMilliseconds, result.Count());
            Assert.True(stopwatch.ElapsedMilliseconds < MAX_EXECUTION_TIME_MS,
                $"Filter took {stopwatch.ElapsedMilliseconds}ms, expected less than {MAX_EXECUTION_TIME_MS}ms");
        }

        [Fact]
        public async Task GetAll_WithLargeDataset_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            await CreateLargeDataset(MEDIUM_DATASET_SIZE);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _productService.GetAll();
            stopwatch.Stop();

            // Assert
            ReportPerformance("Get All Products", stopwatch.ElapsedMilliseconds, result.Count());
            Assert.True(stopwatch.ElapsedMilliseconds < MAX_EXECUTION_TIME_MS,
                $"GetAll took {stopwatch.ElapsedMilliseconds}ms, expected less than {MAX_EXECUTION_TIME_MS}ms");
            Assert.Equal(MEDIUM_DATASET_SIZE, result.Count());
        }

        [Fact]
        public async Task Create_MultipleProducts_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var products = GenerateTestProducts(SMALL_DATASET_SIZE);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            foreach (var product in products)
            {
                await _productService.Create(product);
            }
            stopwatch.Stop();

            // Assert
            ReportPerformance("Create Multiple Products", stopwatch.ElapsedMilliseconds, SMALL_DATASET_SIZE);
            var averageTimePerProduct = stopwatch.ElapsedMilliseconds / SMALL_DATASET_SIZE;
            Assert.True(averageTimePerProduct < 100,
                $"Average time per product creation: {averageTimePerProduct}ms, expected less than 100ms");
        }

        [Fact]
        public async Task Update_MultipleProducts_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            await CreateLargeDataset(SMALL_DATASET_SIZE);
            var products = await _productService.GetAll();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            foreach (var product in products)
            {
                product.Price *= 1.1;
                await _productService.Update(product);
            }
            stopwatch.Stop();

            // Assert
            ReportPerformance("Update Multiple Products", stopwatch.ElapsedMilliseconds, SMALL_DATASET_SIZE);
            var averageTimePerUpdate = stopwatch.ElapsedMilliseconds / SMALL_DATASET_SIZE;
            Assert.True(averageTimePerUpdate < 100,
                $"Average time per update: {averageTimePerUpdate}ms, expected less than 100ms");
        }

        [Fact]
        public async Task ConcurrentOperations_ShouldHandleCorrectly()
        {
            // Arrange
            await CreateLargeDataset(MEDIUM_DATASET_SIZE);
            var tasks = new List<Task>();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var products = await _productService.GetAll();
                    foreach (var product in products.Take(10))
                    {
                        product.Price *= 1.1;
                        await _productService.Update(product);
                    }
                }));
            }
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            ReportPerformance("Concurrent Operations", stopwatch.ElapsedMilliseconds, 100); // 10 tasks * 10 products each
            Assert.True(stopwatch.ElapsedMilliseconds < MAX_EXECUTION_TIME_MS * 2,
                $"Concurrent operations took {stopwatch.ElapsedMilliseconds}ms, expected less than {MAX_EXECUTION_TIME_MS * 2}ms");
        }

        [Fact]
        public async Task Search_WithLargeDataset_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            await CreateLargeDataset(LARGE_DATASET_SIZE);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _productService.Filter(new Dictionary<string, string> { { "name", "Test" } });
            stopwatch.Stop();

            // Assert
            ReportPerformance("Search Operation", stopwatch.ElapsedMilliseconds, result.Count());
            Assert.True(stopwatch.ElapsedMilliseconds < MAX_EXECUTION_TIME_MS,
                $"Search took {stopwatch.ElapsedMilliseconds}ms, expected less than {MAX_EXECUTION_TIME_MS}ms");
        }

        private async Task CreateLargeDataset(int size)
        {
            using var context = _contextFactory.CreateDbContext();
            var products = GenerateTestProducts(size);
            await context.Products.AddRangeAsync(products.Select(p => p.ToEntity()));
            await context.SaveChangesAsync();
        }

        private List<ProductDto> GenerateTestProducts(int count)
        {
            var products = new List<ProductDto>();
            for (int i = 1; i <= count; i++)
            {
                products.Add(new ProductDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Test Product {i}",
                    Description = $"Description for product {i}",
                    Price = 100 + i * 10,
                    SalePrice = 90 + i * 10,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            return products;
        }
    }
}