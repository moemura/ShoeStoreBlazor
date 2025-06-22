namespace WebApp.Models.DTOs;

public class RevenueStatisticDto
{
    public double TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public double AverageOrderValue { get; set; }
    public List<RevenueByTimeDto> RevenueByTime { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

public class RevenueByTimeDto
{
    public string Period { get; set; } // Ngày/Tuần/Tháng/Năm
    public double Revenue { get; set; }
    public int OrderCount { get; set; }
    public DateTime Date { get; set; }
}

public class TopCustomerDto
{
    public string? UserId { get; set; }
    public string CustomerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public double TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public DateTime LastOrderDate { get; set; }
}

public class RevenueFilterRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Period { get; set; } = "day"; // day, week, month, year
    public int TopCustomerCount { get; set; } = 10;
}

// NEW DTOs for enhanced analytics
public class DashboardOverviewDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public List<RevenueByTimeDto> RevenueChart { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<TopBrandDto> TopBrands { get; set; } = new();
    public List<TopCategoryDto> TopCategories { get; set; } = new();
    public List<PaymentMethodStatDto> PaymentMethods { get; set; } = new();
    public List<OrderStatusStatDto> OrderStatuses { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public InventoryStatDto InventoryStats { get; set; } = new();
}

public class DashboardStatsDto
{
    public double TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public double WeekRevenue { get; set; }
    public int WeekOrders { get; set; }
    public double MonthRevenue { get; set; }
    public int MonthOrders { get; set; }
    public double AverageOrderValue { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int LowStockCount { get; set; }
    public double GrowthRate { get; set; }
}

public class TopProductDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string? ProductImage { get; set; }
    public string? BrandName { get; set; }
    public string? CategoryName { get; set; }
    public int TotalSold { get; set; }
    public double TotalRevenue { get; set; }
    public double Price { get; set; }
    public int StockLevel { get; set; }
}

public class TopBrandDto
{
    public string BrandId { get; set; }
    public string BrandName { get; set; }
    public string? Logo { get; set; }
    public int TotalSold { get; set; }
    public double TotalRevenue { get; set; }
    public int ProductCount { get; set; }
    public double AveragePrice { get; set; }
}

public class TopCategoryDto
{
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string? Image { get; set; }
    public int TotalSold { get; set; }
    public double TotalRevenue { get; set; }
    public int ProductCount { get; set; }
    public double MarketShare { get; set; }
}

public class PaymentMethodStatDto
{
    public string PaymentMethod { get; set; }
    public int OrderCount { get; set; }
    public double TotalAmount { get; set; }
    public double Percentage { get; set; }
}

public class OrderStatusStatDto
{
    public string Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class InventoryStatDto
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
}

public class LowStockProductDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string? ProductImage { get; set; }
    public string Size { get; set; }
    public int StockLevel { get; set; }
    public int MinStockLevel { get; set; } = 10; // Threshold for low stock
}

public class DashboardFilterRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TopCount { get; set; } = 10;
    public int LowStockThreshold { get; set; } = 10;
} 