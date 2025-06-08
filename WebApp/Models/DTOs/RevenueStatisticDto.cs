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