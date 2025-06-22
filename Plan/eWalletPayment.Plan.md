# Kế hoạch Tích hợp Thanh toán Ví Điện tử (E-Wallet Payment)

## 1. Tổng quan Hiện tại

### Tình trạng Payment System hiện tại:
- ✅ **PaymentMethod Enum** đã định nghĩa đầy đủ (COD, BankTransfer, CreditCard, MoMo, VnPay, ZaloPay, PayPal)
- ✅ **Payment Strategy Pattern** đã có cấu trúc cơ bản nhưng chưa hoàn thiện
- ✅ **Frontend UI** đã hiển thị các phương thức thanh toán
- ✅ **Configuration** MoMo và VnPay đã có trong appsettings.json
- ❌ **Payment Gateway Integration** chưa được implement
- ❌ **Payment Transaction Tracking** chưa có
- ❌ **Payment Callback/Webhook** chưa có
- ❌ **Payment Status Management** chưa có

### Luồng thanh toán hiện tại:
```
1. User chọn sản phẩm → Thêm vào giỏ hàng
2. Checkout → Điền thông tin + chọn phương thức thanh toán
3. Tạo Order → Gọi PaymentStrategy.ProcessPayment()
4. CODPaymentStrategy → return true (luôn thành công)
5. Trừ kho → Xóa cart → Hoàn thành
```

## 2. Phân tích Yêu cầu

### 2.1 Chức năng cần bổ sung:
1. **Payment Gateway Integration**
   - MoMo API integration
   - VnPay API integration
   - ZaloPay API integration (optional)

2. **Payment Transaction Management**
   - Lưu trữ thông tin giao dịch
   - Tracking payment status
   - Payment history

3. **Payment Workflow Enhancement**
   - Payment URL generation
   - Redirect to payment gateway
   - Handle payment callback/webhook
   - Payment verification
   - Order status update based on payment result

4. **Frontend Enhancements**
   - Payment gateway redirect
   - Payment status polling
   - Payment result handling
   - Loading states for payment process

### 2.2 Luồng thanh toán mới sẽ là:
```
1. User chọn sản phẩm → Checkout → Điền thông tin
2. Chọn E-wallet (MoMo/VnPay) → Tạo Order (Pending Payment)
3. Generate Payment URL → Redirect to Payment Gateway
4. User thanh toán trên gateway → Gateway callback to our system
5. Verify payment → Update order status → Complete order
```

## 3. Kiến trúc Kỹ thuật

### 3.1 Database Schema Changes

#### 3.1.1 Tạo bảng PaymentTransaction
```sql
CREATE TABLE PaymentTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    PaymentMethod INT NOT NULL,
    TransactionId NVARCHAR(255) NULL, -- ID từ payment gateway
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) DEFAULT 'VND',
    Status INT NOT NULL, -- Pending, Success, Failed, Cancelled
    PaymentGatewayResponse NVARCHAR(MAX) NULL, -- JSON response from gateway
    PaymentUrl NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);
```

#### 3.1.2 Thêm enum PaymentTransactionStatus
```csharp
public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Success = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}
```

#### 3.1.3 Cập nhật Order Status
```csharp
public enum OrderStatus
{
    Pending = 1,           // Đang chờ
    PendingPayment = 2,    // Chờ thanh toán (NEW)
    Paid = 3,              // Đã thanh toán (NEW) 
    Preparing = 4,         // Đang chuẩn bị (rename từ Prepairing)
    Shipping = 5,          // Đang giao hàng
    Completed = 6,         // Hoàn thành
    Cancelled = 7,         // Đã hủy
    Rejected = 8           // Bị từ chối
}
```

### 3.2 Backend Components

#### 3.2.1 Models & DTOs
```
Models/Entities/PaymentTransaction.cs
Models/DTOs/PaymentTransactionDto.cs
Models/DTOs/PaymentRequestDto.cs
Models/DTOs/PaymentCallbackDto.cs
Models/DTOs/PaymentResultDto.cs
Models/Mapping/PaymentTransactionMapping.cs
```

#### 3.2.2 Services Layer
```
Services/Payments/
├── IPaymentService.cs
├── PaymentService.cs
├── IPaymentGatewayService.cs
├── MoMoPaymentGatewayService.cs
├── VnPayPaymentGatewayService.cs
├── ZaloPayPaymentGatewayService.cs
└── PaymentGatewayFactory.cs
```

#### 3.2.3 Controllers
```
Controllers/PaymentController.cs - Handle payment endpoints
Controllers/PaymentCallbackController.cs - Handle gateway callbacks
```

#### 3.2.4 Enhanced Payment Strategies
```csharp
// Cập nhật PaymentStrategies.cs
public interface IPaymentStrategy
{
    Task<PaymentResultDto> ProcessPayment(OrderDto order);
}

public class MoMoPaymentStrategy : IPaymentStrategy
{
    public async Task<PaymentResultDto> ProcessPayment(OrderDto order)
    {
        // Tạo PaymentTransaction
        // Generate MoMo payment URL
        // Return payment URL for redirect
    }
}
```

### 3.3 Frontend Components

#### 3.3.1 New Components
```
components/PaymentGatewayRedirect.jsx - Handle payment gateway redirect
components/PaymentStatus.jsx - Show payment status
components/PaymentResult.jsx - Show payment result
pages/PaymentCallback.jsx - Handle payment return
pages/PaymentSuccess.jsx - Payment success page
pages/PaymentFailed.jsx - Payment failed page
```

#### 3.3.2 Enhanced Services
```
services/paymentService.js - Payment API calls
services/paymentGatewayService.js - Payment gateway integration
```

## 4. Implementation Plan

### Phase 1: Database & Models (1-2 ngày)
1. ✅ Tạo migration cho PaymentTransaction table
2. ✅ Tạo PaymentTransaction entity & DTOs
3. ✅ Cập nhật OrderStatus enum
4. ✅ Tạo PaymentTransactionMapping
5. ✅ Update DbContext

### Phase 2: Payment Gateway Services (3-4 ngày)
1. ✅ Implement IPaymentGatewayService interface
2. ✅ Implement MoMoPaymentGatewayService
   - Payment URL generation
   - Signature generation
   - Response verification
3. ✅ Implement VnPayPaymentGatewayService  
   - Payment URL generation
   - Hash verification
   - Response parsing
4. ✅ Create PaymentGatewayFactory
5. ✅ Implement IPaymentService

### Phase 3: Enhanced Payment Strategies (1-2 ngày)
1. ✅ Update IPaymentStrategy interface
2. ✅ Implement new MoMoPaymentStrategy
3. ✅ Implement VnPayPaymentStrategy
4. ✅ Update PaymentStrategyFactory
5. ✅ Update OrderService integration

### Phase 4: Payment Controllers & APIs (2-3 ngày)
1. ✅ Create PaymentController
   - POST /api/payment/process
   - GET /api/payment/status/{transactionId}
   - GET /api/payment/history/{orderId}
2. ✅ Create PaymentCallbackController
   - POST /api/payment/momo-callback
   - GET /api/payment/momo-notify
   - GET /api/payment/vnpay-callback
3. ✅ Update OrderController for payment integration

### Phase 5: Frontend Implementation (3-4 ngày)
1. ✅ Update Checkout.jsx
   - Handle payment method selection
   - Payment gateway redirect logic
2. ✅ Create PaymentGatewayRedirect component
3. ✅ Create payment result pages
4. ✅ Update OrderContext for payment status
5. ✅ Implement payment status polling
6. ✅ Update order history to show payment info

### Phase 6: Testing & Security (2-3 ngày)
1. ✅ Unit tests for payment services
2. ✅ Integration tests for payment flow
3. ✅ Security testing (signature verification)
4. ✅ Error handling & logging
5. ✅ Payment timeout handling

### Phase 7: Production Deployment (1-2 ngày)
1. ✅ Update production configurations
2. ✅ Database migration
3. ✅ Monitor payment transactions
4. ✅ Documentation & training

## 5. Detailed Technical Specifications

### 5.1 MoMo Integration
```csharp
// MoMo API Request
{
    "partnerCode": "MOMO",
    "requestId": "unique-request-id",
    "amount": 100000,
    "orderId": "order-id",
    "orderInfo": "Payment for Order #123",
    "redirectUrl": "https://yoursite.com/payment/momo-callback",
    "ipnUrl": "https://yoursite.com/api/payment/momo-notify",
    "requestType": "captureMoMoWallet",
    "extraData": "",
    "signature": "generated-signature"
}
```

### 5.2 VnPay Integration
```csharp
// VnPay Payment URL Parameters
{
    "vnp_Version": "2.1.0",
    "vnp_Command": "pay", 
    "vnp_TmnCode": "merchant-code",
    "vnp_Amount": "10000000", // VND * 100
    "vnp_CurrCode": "VND",
    "vnp_TxnRef": "transaction-ref",
    "vnp_OrderInfo": "Payment for order",
    "vnp_ReturnUrl": "callback-url",
    "vnp_CreateDate": "yyyyMMddHHmmss",
    "vnp_SecureHash": "generated-hash"
}
```

### 5.3 Payment Flow Security
1. **Signature Verification**: Verify all callbacks with proper signatures
2. **Transaction Validation**: Double-check transaction amounts and order IDs
3. **Idempotency**: Handle duplicate callback calls
4. **Timeout Handling**: Handle payment timeout scenarios
5. **Logging**: Comprehensive logging for audit trails

## 6. Configuration Updates

### 6.1 appsettings.json Enhancements
```json
{
  "PaymentSettings": {
    "DefaultTimeout": 900, // 15 minutes
    "MaxRetries": 3,
    "EnabledGateways": ["MoMo", "VnPay"],
    "BaseCallbackUrl": "https://yoursite.com"
  },
  "MoMoSettings": {
    // existing config +
    "TimeoutMinutes": 15,
    "MaxAmount": 50000000
  },
  "VnPaySettings": {  
    // existing config +
    "TimeoutMinutes": 15,
    "MaxAmount": 50000000
  }
}
```

### 6.2 Service Registration
```csharp
// ServiceContainer.cs
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IPaymentGatewayService, MoMoPaymentGatewayService>();
services.AddScoped<IPaymentGatewayService, VnPayPaymentGatewayService>();
services.AddScoped<PaymentGatewayFactory>();
```

## 7. Risk Assessment & Mitigation

### 7.1 Technical Risks
| Risk | Impact | Mitigation |
|------|---------|------------|
| Payment Gateway Downtime | High | Implement fallback to COD, retry logic |
| Callback Security Breach | Critical | Strong signature verification, IP whitelisting |
| Transaction Duplication | Medium | Idempotency keys, transaction tracking |
| Network Timeout | Medium | Proper timeout handling, status polling |

### 7.2 Business Risks
| Risk | Impact | Mitigation |
|------|---------|------------|
| Payment Gateway Fees | Medium | Monitor transaction volumes, negotiate rates |
| User Experience Issues | High | Thorough testing, clear error messages |
| Compliance Issues | Critical | Follow PCI DSS guidelines, data protection |

## 8. Monitoring & Analytics

### 8.1 Key Metrics to Track
- Payment success rate by gateway
- Average payment completion time
- Payment failure reasons
- Transaction volumes by method
- User drop-off rates in payment flow

### 8.2 Logging Requirements
- All payment gateway requests/responses
- Payment status changes
- Error scenarios with detailed context
- User actions in payment flow

## 9. Post-Implementation Tasks

### 9.1 Immediate (Week 1)
- Monitor payment success rates
- Fix any critical bugs
- Gather user feedback
- Performance optimization

### 9.2 Short-term (Month 1)
- Implement ZaloPay integration
- Add payment analytics dashboard  
- Optimize payment UX based on feedback
- Implement payment refund functionality

### 9.3 Long-term (Quarter 1)
- Add international payment methods
- Implement installment payments
- Advanced fraud detection
- Payment method recommendations

## 10. Success Criteria

### 10.1 Technical Success
- ✅ 99%+ payment gateway uptime
- ✅ <5 second payment redirect time
- ✅ 100% callback signature verification
- ✅ Zero payment data breaches

### 10.2 Business Success  
- ✅ >90% payment success rate
- ✅ <2% user drop-off in payment flow
- ✅ 50%+ orders use e-wallet payments
- ✅ Positive user feedback on payment experience

---

**Người thực hiện**: Development Team  
**Thời gian ước tính**: 12-16 ngày làm việc  
**Priority**: High  
**Dependencies**: Database migration, Payment gateway account setup
