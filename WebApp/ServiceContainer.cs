using WebApp.Services.Auth;
using WebApp.Services.Brands;
using WebApp.Services.Carts;
using WebApp.Services.Catches;
using WebApp.Services.Categories;
using WebApp.Services.Files;
using WebApp.Services.Orders;
using WebApp.Services.Products;
using WebApp.Services.Sizes;

namespace WebApp;

public static class ServiceContainer
{
    public static void AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<OrderTotalStrategyFactory>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        //services.AddScoped<ICacheService, NoCacheService>();
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ISizeService, SizeService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IImageStorageService, ImgurService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddHttpClient();
    }
    public static void AddExternalServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddAntDesign();
        services.AddScoped<IEmailSender, EmailSender>();
    }
}
