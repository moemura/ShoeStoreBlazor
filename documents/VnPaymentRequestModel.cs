namespace Infrastructure.Services.VnPay
{
    public class VnPaymentRequestModel
    {
        public string FullName { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public int BookingId { get; set; }
    }
}
