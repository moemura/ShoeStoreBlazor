using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;
using WebApp.Services.Auth;

namespace WebApp.Tests.DataTests;

public class AuthServiceTest
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
    private readonly Mock<IOptions<JwtSettings>> _jwtOptionsMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTest()
    {
        var options = new DbContextOptionsBuilder<ShoeStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ShoeStoreDbContext(options);
        _dbContextFactory = new TestDbContextFactory(dbContext);

        _userManagerMock = MockUserManager();
        _roleManagerMock = MockRoleManager();
        _signInManagerMock = MockSignInManager(_userManagerMock.Object);
        _jwtSettings = new JwtSettings
        {
            SecretKey = "supersecretkey1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60,
            RefreshTokenExpiryInDays = 7
        };
        _jwtOptionsMock = new Mock<IOptions<JwtSettings>>();
        _jwtOptionsMock.Setup(x => x.Value).Returns(_jwtSettings);
        _emailSenderMock = new Mock<IEmailSender>();
    }

    private AuthService CreateService() => new AuthService(
        _userManagerMock.Object,
        _roleManagerMock.Object,
        _signInManagerMock.Object,
        _jwtOptionsMock.Object,
        _dbContextFactory,
        _emailSenderMock.Object
    );

    [Fact]
    public async Task RegisterAsync_Success()
    {
        // Arrange
        var service = CreateService();
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            PhoneNumber = "0123456789"
        };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(x => x.RoleExistsAsync("Customer")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"))
            .ReturnsAsync(IdentityResult.Success);
        // Act
        var result = await service.RegisterAsync(registerDto);
        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_Fail_WhenUserExists()
    {
        // Arrange
        var service = CreateService();
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            PhoneNumber = "0123456789"
        };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email exists" }));
        // Act
        var result = await service.RegisterAsync(registerDto);
        // Assert
        Assert.False(result.Success);
        Assert.Contains("Email exists", result.Message);
    }

    // Các test khác tương tự, ví dụ cho LoginAsync, RefreshTokenAsync, v.v.

    // Helper methods for mocking
    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
    }
    private static Mock<RoleManager<IdentityRole>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
    }
    private static Mock<SignInManager<AppUser>> MockSignInManager(UserManager<AppUser> userManager)
    {
        var context = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        return new Mock<SignInManager<AppUser>>(userManager, null, null, null, null, null, null);
    }
    private class TestDbContextFactory : IDbContextFactory<ShoeStoreDbContext>
    {
        private readonly ShoeStoreDbContext _context;
        public TestDbContextFactory(ShoeStoreDbContext context) => _context = context;
        public ShoeStoreDbContext CreateDbContext() => _context;
        public Task<ShoeStoreDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(_context);
    }
} 