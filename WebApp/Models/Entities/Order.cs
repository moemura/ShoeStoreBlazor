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
    Pending = 1,           // Đang chờ
    PendingPayment = 2,    // Chờ thanh toán
    Paid = 3,              // Đã thanh toán
    Preparing = 4,         // Đang chuẩn bị
    Shipping = 5,          // Đang giao hàng
    Completed = 6,         // Hoàn thành
    Cancelled = 7,         // Đã hủy
    Rejected = 8           // Bị từ chối
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
