# 📋 Quản lý Đơn hàng (Order Management)

## 🎯 Tổng quan
Chức năng quản lý đơn hàng cho admin web, cho phép xem, lọc, cập nhật trạng thái và hủy đơn hàng.

## 🌐 URL
- **Route**: `/orders`
- **Authorization**: Yêu cầu quyền Admin (`RequireAdminRole`)

## ✨ Tính năng chính

### 1. **Hiển thị danh sách đơn hàng**
- Bảng hiển thị thông tin đầy đủ: mã đơn, khách hàng, tổng tiền, trạng thái, phương thức thanh toán, ngày tạo
- Phân trang linh hoạt: 5/10/20/50 đơn/trang
- Tự động refresh khi có thay đổi

### 2. **Tìm kiếm & Lọc**
```
🔍 Tìm kiếm:
├── Theo tên khách hàng
└── Theo số điện thoại

📊 Bộ lọc:
├── Trạng thái đơn hàng
├── Phương thức thanh toán
├── Khoảng thời gian
└── Khoảng giá tiền
```

### 3. **Xem chi tiết đơn hàng**
- **Thông tin khách hàng**: Tên, SĐT, email, địa chỉ, ghi chú
- **Thông tin đơn hàng**: Mã đơn, ngày tạo, trạng thái, thanh toán, voucher
- **Danh sách sản phẩm**: Ảnh, tên, size, giá, số lượng, thành tiền
- **Tổng tiền** được tính toán và hiển thị rõ ràng

### 4. **Cập nhật trạng thái**
- **Logic chuyển trạng thái**:
  - `Pending` → `Preparing` / `Cancelled` / `Rejected`
  - `Preparing` → `Shipping` / `Cancelled`
  - `Shipping` → `Completed`
  - `Completed/Cancelled/Rejected` → Không thể chuyển
- **Thêm ghi chú** khi cập nhật (tùy chọn)
- **Xác nhận** trước khi thực hiện

### 5. **Hủy đơn hàng**
- Chỉ cho phép hủy đơn ở trạng thái `Pending` hoặc `Preparing`
- **Popconfirm** xác nhận trước khi hủy
- Tự động thêm ghi chú "Hủy bởi admin"

## 🎨 UI/UX Features

### **Màu sắc trạng thái**
- 🟡 **Pending**: Vàng (warning)
- 🔵 **Preparing**: Xanh (processing)
- 🟢 **Shipping**: Xanh lá (success)
- ✅ **Completed**: Xanh đậm (success)
- 🔴 **Cancelled**: Đỏ (error)
- ⚫ **Rejected**: Xám (default)

### **Loading States**
- Loading spinner khi tải dữ liệu
- Disable buttons trong lúc xử lý
- Loading indicator cho các thao tác async

### **Responsive Design**
- Table responsive trên mobile
- Modal tự động điều chỉnh kích thước
- Icon và button size phù hợp

## 🔧 Technical Stack

### **Services sử dụng**
```csharp
@inject IOrderService orderService
@inject MessageService MessageService
```

### **AntDesign Components**
- `Table` - Hiển thị danh sách
- `Modal` - Chi tiết, lọc, cập nhật
- `Form` - Input forms
- `Select/Input` - Form controls
- `Badge/Tag` - Status display
- `Descriptions` - Chi tiết đơn hàng
- `Popconfirm` - Xác nhận thao tác

### **Key Methods từ OrderService**
```csharp
- FilterAndPaging() // Lọc & phân trang
- GetOrderById()    // Chi tiết đơn hàng
- UpdateOrderStatus() // Cập nhật trạng thái
```

## 📝 Notes
- **Performance**: Sử dụng pagination để tối ưu hiệu suất
- **Security**: Authorization policy đảm bảo chỉ admin mới truy cập
- **UX**: Luôn hiển thị loading states và success/error messages
- **Data Integrity**: Validate trạng thái trước khi cập nhật 