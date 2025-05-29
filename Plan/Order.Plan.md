# Order Module Backend Plan

## 1. Entity Design

- **Order**
  - Id (Guid)
  - UserId (nullable, cho phép guest)
  - CustomerName, Phone, Email, Address (lưu snapshot khi đặt hàng)
  - TotalAmount, PaymentMethod, Status, Note
  - CreatedAt, UpdatedAt
  - List<OrderItem>

- **OrderItem**
  - Id (Guid)
  - OrderId (FK)
  - InventoryId (FK, Product+Size)
  - ProductName, Size, Price, Quantity, Subtotal

## 2. DTOs
- **OrderDto**: Thông tin đơn hàng, list OrderItemDto, trạng thái, tổng tiền, thông tin khách.
- **OrderItemDto**: InventoryId, ProductName, Size, Price, Quantity, Subtotal.
- **OrderCreateRequest**: Thông tin khách, địa chỉ, payment method, note, list item (InventoryId, Quantity).

## 3. Service
- **IOrderService**
  - Task<OrderDto> CreateOrder(OrderCreateRequest req, string? userId, string? guestId)
  - Task<IEnumerable<OrderDto>> GetOrdersByUser(string userId)
  - Task<OrderDto> GetOrderById(string orderId)
  - Task UpdateOrderStatus(string orderId, string status)
- **OrderService**
  - Tạo mới order từ cart (user hoặc guest), kiểm tra tồn kho, trừ kho, lưu order, xóa cart.
  - Lưu snapshot thông tin sản phẩm, giá, size vào OrderItem.
  - Hỗ trợ thanh toán COD hoặc MoMo (tích hợp sau).

## 4. Controller
- **OrderController**
  - API tạo đơn hàng (checkout, cho phép guest và user)
  - API lấy danh sách đơn hàng của user (yêu cầu JWT)
  - API lấy chi tiết đơn hàng
  - API cập nhật trạng thái đơn hàng (cho admin)

## 5. Logic & Flow
- Khi user/guest checkout:
  1. FE gửi OrderCreateRequest (lấy từ cart FE hoặc BE)
  2. BE kiểm tra tồn kho từng item, nếu đủ thì trừ kho, tạo order, lưu OrderItem snapshot.
  3. Nếu là guest, lưu thông tin khách vào order, không gắn userId.
  4. Xóa cart sau khi đặt hàng thành công.
  5. Trả về OrderDto.
- Đơn hàng có thể thanh toán COD hoặc MoMo (tích hợp sau).
- Đơn hàng lưu trạng thái: Pending, Paid, Shipping, Completed, Cancelled.

## 6. Implementation Steps
1. Tạo entity Order, OrderItem trong Models/Entities
2. Thêm DbSet vào ShoeStoreDbContext
3. Tạo DTOs trong Models/DTOs
4. Mapping thủ công trong Models/Mapping
5. Tạo OrderService, IOrderService trong Services/Orders
6. Đăng ký DI trong ServiceContainer.cs
7. Tạo OrderController trong Controllers

## 7. Notes
- Đảm bảo kiểm tra tồn kho và trừ kho atomically khi tạo order.
- Lưu snapshot giá, tên sản phẩm, size vào OrderItem để không bị thay đổi khi sản phẩm cập nhật.
- Hỗ trợ guest checkout, không yêu cầu đăng nhập.
- Áp dụng Strategy Pattern và Factory Method để Có thể mở rộng cho mã giảm giá, khuyến mãi, thanh toán online và xử lý trạng thái dựa vào hình thức thanh toán.
