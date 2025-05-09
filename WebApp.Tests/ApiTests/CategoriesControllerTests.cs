using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Data;
using WebApp.Endpoints;
using WebApp.Models;
using WebApp.Models.DTOs;
using Xunit;

namespace WebApp.Tests.ApiTests
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoriesController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            // Arrange
            var expectedCategories = new List<CategoryDto>
            {
                new() { Name = "Category 1" },
                new() { Name = "Category 2" }
            };
            _mockCategoryService.Setup(x => x.GetAll())
                .ReturnsAsync(expectedCategories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
            Assert.Equal(expectedCategories, returnValue);
        }

        [Fact]
        public async Task GetAll_WithPagination_ReturnsOkResult()
        {
            // Arrange
            var index = 1;
            var size = 10;
            var expectedPagination = new PaginationData<CategoryDto>
            {
                Data = new List<CategoryDto> { new() { Name = "Category 1" } },
                PageIndex = index,
                PageSize = size,
                ItemCount = 1,
                PageCount = 1
            };
            _mockCategoryService.Setup(x => x.GetPagination(index, size))
                .ReturnsAsync(expectedPagination);

            // Act
            var result = await _controller.GetAll(index, size);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<PaginationData<CategoryDto>>(okResult.Value);
            Assert.Equal(expectedPagination, returnValue);
        }

        [Fact]
        public async Task Get_ExistingCategory_ReturnsOkResult()
        {
            // Arrange
            var expectedCategory = new CategoryDto { Id = "1", Name = "Test Category" };
            _mockCategoryService.Setup(x => x.GetById("1"))
                .ReturnsAsync(expectedCategory);

            // Act
            var result = await _controller.Get("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal(expectedCategory, returnValue);
        }

        [Fact]
        public async Task Get_NonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            _mockCategoryService.Setup(x => x.GetById("non-existing-id"))
                .ReturnsAsync((CategoryDto)null);

            // Act
            var result = await _controller.Get("non-existing-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidCategory_ReturnsOkResult()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Test Category" };
            var expectedCategory = new CategoryDto { Id = "1", Name = "Test Category" };
            _mockCategoryService.Setup(x => x.Create(categoryDto))
                .ReturnsAsync(expectedCategory);

            // Act
            var result = await _controller.Create(categoryDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal(expectedCategory, returnValue);
        }

        [Fact]
        public async Task Update_ExistingCategory_ReturnsOkResult()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = "1", Name = "Updated Category" };
            _mockCategoryService.Setup(x => x.Update(categoryDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(categoryDto);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockCategoryService.Verify(x => x.Update(categoryDto), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingCategory_ReturnsOkResult()
        {
            // Arrange
            _mockCategoryService.Setup(x => x.Delete("1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<OkResult>(result);
            _mockCategoryService.Verify(x => x.Delete("1"), Times.Once);
        }
    }
} 