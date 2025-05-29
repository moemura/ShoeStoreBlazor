using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApp.BlazorPages.Base;
using WebApp.Services.Auth;
using WebApp.Services.Catches;
using WebApp.Services.Carts;

namespace WebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRazorPages();

        // Configure JWT settings
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        
        if (string.IsNullOrEmpty(jwtSettings?.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured. Please set a valid secret key in appsettings.json");
        }

        if (jwtSettings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
        }

        // Add Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<ShoeStoreDbContext>()
        .AddDefaultTokenProviders();

        // Add Authentication with both Cookie and JWT
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/admin/login";
            options.LogoutPath = "/admin/logout";
            options.AccessDeniedPath = "/access-denied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.Cookie.Name = "ShoeStore.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings!.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        // Add Authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin"));
        });

        // Add services before other configurations
        builder.Services.AddAppServices();
        builder.Services.AddExternalServices();

        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Register DbContext and DbContextFactory with proper scoping
        builder.Services.AddDbContextFactory<ShoeStoreDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        }, ServiceLifetime.Scoped);

        builder.Services.AddDbContext<ShoeStoreDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
        }, ServiceLifetime.Scoped);

        builder.Services.AddScoped<DbSeeder>();



        var app = builder.Build();

        // Seed the database
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var seeder = services.GetRequiredService<DbSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();
        app.MapControllers();
        app.UseCors(o =>
        {
            o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapRazorPages();
        await app.RunAsync();
    }
}
