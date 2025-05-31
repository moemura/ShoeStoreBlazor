using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Endpoints;
using WebApp.Models.DTOs;
using WebApp.Services.Categories;

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
    }
} 