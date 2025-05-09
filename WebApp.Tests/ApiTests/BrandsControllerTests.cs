using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Data;
using WebApp.Endpoints;
using WebApp.Models;
using WebApp.Models.DTOs;

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

        [Fact]
        public async Task GetAll_WithPagination_ReturnsOkResult()
        {
            // Arrange
            var index = 1;
            var size = 10;
            var expectedPagination = new PaginationData<BrandDto>
            {
                Data = new List<BrandDto> { new() { Name = "Brand 1" } },
                PageIndex = index,
                PageSize = size,
                ItemCount = 1,
                PageCount = 1
            };
            _mockBrandService.Setup(x => x.GetPagination(index, size))
                .ReturnsAsync(expectedPagination);

            // Act
            var result = await _controller.GetAll(index, size);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<PaginationData<BrandDto>>(okResult.Value);
            Assert.Equal(expectedPagination, returnValue);
        }

        [Fact]
        public async Task Get_ExistingBrand_ReturnsOkResult()
        {
            // Arrange
            var expectedBrand = new BrandDto { Id = "1", Name = "Test Brand" };
            _mockBrandService.Setup(x => x.GetById("1"))
                .ReturnsAsync(expectedBrand);

            // Act
            var result = await _controller.Get("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<BrandDto>(okResult.Value);
            Assert.Equal(expectedBrand, returnValue);
        }

        [Fact]
        public async Task Get_NonExistingBrand_ReturnsNotFound()
        {
            // Arrange
            _mockBrandService.Setup(x => x.GetById("non-existing-id"))
                .ReturnsAsync((BrandDto)null);

            // Act
            var result = await _controller.Get("non-existing-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidBrand_ReturnsOkResult()
        {
            // Arrange
            var brandDto = new BrandDto { Name = "Test Brand" };
            var expectedBrand = new BrandDto { Id = "1", Name = "Test Brand" };
            _mockBrandService.Setup(x => x.Create(brandDto))
                .ReturnsAsync(expectedBrand);

            // Act
            var result = await _controller.Create(brandDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<BrandDto>(okResult.Value);
            Assert.Equal(expectedBrand, returnValue);
        }

        [Fact]
        public async Task Update_ExistingBrand_ReturnsOkResult()
        {
            // Arrange
            var brandDto = new BrandDto { Id = "1", Name = "Updated Brand" };
            _mockBrandService.Setup(x => x.Update(brandDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(brandDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockBrandService.Verify(x => x.Update(brandDto), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingBrand_ReturnsOkResult()
        {
            // Arrange
            _mockBrandService.Setup(x => x.Delete("1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<OkResult>(result);
            _mockBrandService.Verify(x => x.Delete("1"), Times.Once);
        }
    }
} 