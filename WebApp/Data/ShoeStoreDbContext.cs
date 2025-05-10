using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models.Entities;

namespace WebApp.Data;

public class ShoeStoreDbContext : IdentityDbContext<AppUser>
{
    public ShoeStoreDbContext(DbContextOptions<ShoeStoreDbContext> options) : base(options) { }
    public ShoeStoreDbContext() { }

    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Size> Sizes { get; set; }
    public virtual DbSet<Inventory> Inventories { get; set; }
}
