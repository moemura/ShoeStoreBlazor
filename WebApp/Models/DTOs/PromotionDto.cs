namespace WebApp.Models.DTOs;

public class PromotionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string Scope { get; set; } = string.Empty;
    public double? MinOrderAmount { get; set; }
    public List<string> ProductIds { get; set; } = new();
    public List<string> CategoryIds { get; set; } = new();
    public List<string> BrandIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // For Blazor DateRange picker
    public DateTime[]? DateRange { get; set; }
}

public class CreatePromotionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; } = 1;
    public string Scope { get; set; } = "All";
    public double? MinOrderAmount { get; set; }
    public List<string> ProductIds { get; set; } = new();
    public List<string> CategoryIds { get; set; } = new();
    public List<string> BrandIds { get; set; } = new();
} 