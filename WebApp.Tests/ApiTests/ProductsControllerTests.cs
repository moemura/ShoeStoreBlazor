using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Endpoints;
using WebApp.Models;
using WebApp.Models.DTOs;
using WebApp.Services.Products;
using WebApp.Services.Promotions;

namespace WebApp.Tests.ApiTests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IPromotionService> _mockPromotionService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockPromotionService = new Mock<IPromotionService>();
            _controller = new ProductsController(_mockProductService.Object, _mockPromotionService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithProducts()
        {
            // Arrange
            var expectedProducts = new List<ProductDto>
            {
                new() { Id = "1", Name = "Test Product 1", Description = "Test", Price = 100 },
                new() { Id = "2", Name = "Test Product 2", Description = "Test", Price = 200 }
            };
            _mockProductService.Setup(x => x.GetAll())
                .ReturnsAsync(expectedProducts);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
            Assert.Equal(expectedProducts[0].Name, returnValue.First().Name);
        }

        [Fact]
        public async Task GetAll_WithNoProducts_ShouldReturnEmptyList()
        {
            // Arrange
            _mockProductService.Setup(x => x.GetAll())
                .ReturnsAsync(new List<ProductDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetPagination_ShouldReturnOkWithPaginationData()
        {
            // Arrange
            var expectedData = new PaginatedList<ProductDto>
            {
                Data = new List<ProductDto>
                {
                    new() { Id = "1", Name = "Test Product 1", Description = "Test", Price = 100 },
                    new() { Id = "2", Name = "Test Product 2", Description = "Test", Price = 200 }
                },
                PageIndex = 1,
                PageSize = 2,
                ItemCount = 2,
                PageCount = 1,
                HasNext = false,
                HasPrevious = false
            };

            _mockProductService.Setup(x => x.GetPagination(1, 2))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.GetAll(1, 2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginatedList<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal(1, returnValue.PageIndex);
            Assert.Equal(2, returnValue.PageSize);
            Assert.Equal(2, returnValue.ItemCount);
        }

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnOkWithProduct()
        {
            // Arrange
            var expectedProduct = new ProductDto
            {
                Id = "1",
                Name = "Test Product",
                Description = "Test",
                Price = 100
            };

            _mockProductService.Setup(x => x.GetById("1"))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.Get("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("1", returnValue.Id);
            Assert.Equal("Test Product", returnValue.Name);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            _mockProductService.Setup(x => x.GetById("invalid-id"))
                .ReturnsAsync((ProductDto)null);

            // Act
            var result = await _controller.Get("invalid-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
} 