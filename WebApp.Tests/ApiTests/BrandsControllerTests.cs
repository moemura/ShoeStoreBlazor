using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Endpoints;
using WebApp.Models;
using WebApp.Models.DTOs;
using WebApp.Services.Brands;

namespace WebApp.Tests.ApiTests
{
    public class BrandsControllerTests
    {
        private readonly Mock<IBrandService> _mockBrandService;
        private readonly BrandsController _controller;

        public BrandsControllerTests()
        {
            _mockBrandService = new Mock<IBrandService>();
            _controller = new BrandsController(_mockBrandService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            // Arrange
            var expectedBrands = new List<BrandDto>
            {
                new() { Name = "Brand 1" },
                new() { Name = "Brand 2" }
            };
            _mockBrandService.Setup(x => x.GetAll())
                .ReturnsAsync(expectedBrands);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<BrandDto>>(okResult.Value);
            Assert.Equal(expectedBrands, returnValue);
        }
    }
} 