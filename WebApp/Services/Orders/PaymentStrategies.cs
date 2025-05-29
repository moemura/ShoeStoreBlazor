namespace WebApp.Services.Orders;

public interface IPaymentStrategy
{
    Task<bool> ProcessPayment(OrderDto order);
}

public class CODPaymentStrategy : IPaymentStrategy
{
    public Task<bool> ProcessPayment(OrderDto order)
    {
        // Thanh toán khi nhận hàng, luôn true
        return Task.FromResult(true);
    }
}

public class MoMoPaymentStrategy : IPaymentStrategy
{
    public Task<bool> ProcessPayment(OrderDto order)
    {
        // Tích hợp MoMo ở đây
        throw new NotImplementedException();
    }
}

// Thêm các strategy khác tương tự...

public static class PaymentStrategyFactory
{
    public static IPaymentStrategy GetStrategy(int paymentMethod)
    {
        return paymentMethod switch
        {
            0 => new CODPaymentStrategy(),
            3 => new MoMoPaymentStrategy(),
            // Thêm các case khác
            _ => new CODPaymentStrategy()
        };
    }
}
