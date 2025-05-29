namespace WebApp.Models.Entities;

public class Order : BaseEntity<Guid>
{
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
    public string? HandlerId { get; set; }
    public string CustomerName { get; set; }
    public string Phone { get; set; }
    public string? Email { get; set; }
    public string Address { get; set; }
    public string? VoucherCode { get; set; }
    public double TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerNote { get; set; }
    public string? Note { get; set; }
    public virtual ICollection<OrderItem>? Items { get; set; }
    public virtual AppUser? User { get; set; }
    public virtual AppUser? Handler { get; set; }
}

public enum OrderStatus
{
    Pending = 1,
    Prepairing = 2,
    Shipping = 3,
    Completed = 4,
    Cancelled = 5,
    Rejected = 6
}

public enum PaymentMethod
{
    COD = 0,
    BankTranfer = 1,
    CreditCard = 2,
    MoMo = 3,
    VnPay = 4,
    ZaloPay = 5,
    PayPal = 6
}
