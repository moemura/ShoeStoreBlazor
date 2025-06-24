namespace WebApp.Services.Promotions;

public static class PromotionCalculator
{
    public static double CalculateDiscountedPrice(double originalPrice, PromotionType type, double discountValue, double? maxDiscount = null)
    {
        switch (type)
        {
            case PromotionType.Percentage:
                var percentageDiscount = originalPrice * (discountValue / 100);
                if (maxDiscount.HasValue && percentageDiscount > maxDiscount.Value)
                    percentageDiscount = maxDiscount.Value;
                return Math.Max(0, originalPrice - percentageDiscount);
                
            case PromotionType.FixedAmount:
                var fixedDiscount = Math.Min(discountValue, originalPrice);
                return Math.Max(0, originalPrice - fixedDiscount);
                
            case PromotionType.BuyXGetY:
                // Future enhancement - for now return original price
                return originalPrice;
                
            default:
                return originalPrice;
        }
    }

    public static double CalculateDiscount(double originalPrice, PromotionType type, double discountValue, double? maxDiscount = null)
    {
        var discountedPrice = CalculateDiscountedPrice(originalPrice, type, discountValue, maxDiscount);
        return originalPrice - discountedPrice;
    }

    public static bool IsPromotionValid(Promotion promotion, DateTime? checkDate = null)
    {
        var now = checkDate ?? DateTime.UtcNow;
        return promotion.IsActive && 
               promotion.StartDate <= now && 
               promotion.EndDate >= now;
    }

    public static Promotion? GetBestPromotion(IEnumerable<Promotion> promotions, double originalPrice)
    {
        var validPromotions = promotions.Where(p => IsPromotionValid(p));
        
        if (!validPromotions.Any())
            return null;

        // Sort by priority (higher first), then by discount amount (higher first)
        return validPromotions
            .OrderByDescending(p => p.Priority)
            .ThenByDescending(p => CalculateDiscount(originalPrice, p.Type, p.DiscountValue, p.MaxDiscountAmount))
            .FirstOrDefault();
    }
} 