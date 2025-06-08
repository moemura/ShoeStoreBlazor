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

## Tính năng UX mới được tối ưu

### 1. Nút Reload
- **Vị trí**: Thanh công cụ chính
- **Tính năng**: Tải lại dữ liệu ngay lập tức với loading indicator
- **UX**: Feedback thành công "Đã tải lại dữ liệu!" sau khi hoàn thành

### 2. Responsive Filter Design
- **Desktop (>991px)**: Hiển thị filter inline trực tiếp trên page
- **Tablet/Mobile (≤991px)**: Ẩn filter inline, hiển thị nút "Lọc" mở modal
- **Tự động responsive**: Không cần cấu hình thêm

#### Filter Inline (Desktop)
- Card với background nhẹ (#fafafa)
- 4 cột filter: Trạng thái, Phương thức TT, Khoảng thời gian, Khoảng giá
- Real-time filtering với debounce 300ms
- Hover effect với shadow

#### Filter Modal (Mobile)
- Modal popup với form layout truyền thống
- Đầy đủ các options như desktop
- Tối ưu cho touch interface

### 3. Date Range Filter với UX tốt hơn
- **Inline position**: Hiển thị ngay trong hàng filter (desktop)
- **Smart placeholder**: Hiển thị tuần hiện tại thay vì "01-01-0001"
  - Từ: Thứ 2 tuần hiện tại
  - Đến: Ngày hiện tại
- **DefaultPickerValue**: Picker mở ở tháng hiện tại (giải quyết vấn đề năm 0001)
- **Quick Date Buttons**: Nút nhanh cho các khoảng thời gian phổ biến
  - Desktop: Hôm nay, 7 ngày, Tháng này
  - Modal: Hôm nay, 7 ngày qua, 30 ngày qua, Tháng này, Tháng trước
- **Format**: dd/MM/yyyy (phù hợp với người Việt)
- **Real-time**: Tự động filter khi chọn ngày

## Responsive Breakpoints

```css
/* Desktop: >= 992px */
- Hiển thị filter inline
- Ẩn nút "Lọc" modal
- Hiển thị text đầy đủ cho buttons

/* Tablet: 576px - 991px */  
- Ẩn filter inline
- Hiển thị nút "Lọc" modal
- Rút gọn text buttons

/* Mobile: <= 575px */
- Layout stack vertical
- Ẩn text buttons, chỉ hiển thị icon
- Center align controls
```

## Các thành phần UI

### Search & Controls Row
```razor
<Row Gutter="16">
  <Col Xs="24" Sm="12" Md="8" Lg="6">
    <!-- Search Input với icon -->
  </Col>
  <Col Xs="24" Sm="12" Md="8" Lg="6">
    <!-- Reload, Filter Modal, Clear buttons -->
  </Col>
  <Col Xs="24" Sm="12" Md="8" Lg="6">
    <!-- Page size selector -->
  </Col>
</Row>
```

### Desktop Filter Card
```razor
<Card Class="filter-card desktop-filters">
  <Row Gutter="16">
    <Col Span="6">Status Select</Col>
    <Col Span="6">Payment Method Select</Col>
    <Col Span="6">Date Range Picker</Col>
    <Col Span="6">Price Range Input</Col>
  </Row>
</Card>
```

## JavaScript/CSS Features

### Animations
- Button hover transform: `translateY(-1px)`
- Card hover shadow effect
- Smooth transitions: `all 0.3s ease`

### Responsive Classes
- `.hidden-xs`: Ẩn trên mobile
- `.desktop-filters`: Chỉ hiện desktop
- `.filter-modal-btn`: Chỉ hiện mobile
- `.d-flex`, `.justify-content-end`: Flex utilities

## Performance Optimizations

### Debouncing
- Filter changes debounce 300ms
- Giảm số lần gọi API khi user typing

### Smart Loading
- Loading state cho reload button
- Loading state cho table khi filtering

### Memory Management
- Clear filter states properly
- Dispose event handlers

## Event Handlers

### Filter Events
```csharp
// Select changes
OnSelectedItemChanged="@(async (OrderStatus? value) => await OnInlineFilterChange())"

// Date range changes  
OnChange="@(async (DateRangeChangedEventArgs<DateTime[]> args) => await OnInlineFilterChange())"

// Number input changes
OnChange="@(async (double? value) => await OnInlineFilterChange())"
```

### Date Placeholder & Default Value Logic
```csharp
private string[] GetDatePlaceholder()
{
    var today = DateTime.Now;
    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
    return new[] { 
        startOfWeek.ToString("dd/MM/yyyy"), 
        today.ToString("dd/MM/yyyy") 
    };
}

private DateTime[] GetDefaultPickerValue()
{
    var today = DateTime.Now;
    return new[] { today.AddDays(-7), today }; // Picker mở ở tháng hiện tại
}
```

### Quick Date Selection
```csharp
// Inline filter quick selection
private async Task SetDateRange(DateTime[] range)
{
    _inlineFilterModel.DateRange = range;
    await OnInlineFilterChange();
}

// Modal filter quick selection  
private void SetModalDateRange(DateTime[] range)
{
    _filterModel.DateRange = range;
}
```

## Accessibility Features

### Keyboard Navigation
- Tab order tối ưu
- Enter/Space support trên buttons
- Arrow keys trong dropdowns

### Screen Reader
- Semantic HTML structure
- Proper ARIA labels
- Descriptive button text

### Touch Friendly
- 44px minimum touch targets
- Adequate spacing between elements
- Swipe gestures support

## Browser Support
- Chrome/Edge: Full support
- Firefox: Full support  
- Safari: Full support
- Mobile browsers: Optimized

## Future Enhancements
1. Save filter preferences to localStorage
2. Quick filter presets (Today, This week, This month)
3. Advanced search with multiple criteria
4. Export filtered results
5. Bulk actions on filtered items

## Notes
- CSS sử dụng `@@media` syntax cho Blazor
- Event handlers có proper async/await
- All filters clear together với single action
- Maintains existing functionality while improving UX 