# 👟 ShoeStore - Hệ thống Quản lý Cửa hàng Giày

<div align="center">

![ShoeStore Logo](WebApp/wwwroot/logo.png)

**Hệ thống thương mại điện tử hoàn chỉnh cho cửa hàng giày dép**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.2.0-61DAFB)](https://reactjs.org/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927)](https://www.microsoft.com/sql-server)

</div>

## 📋 Mục lục

- [🎯 Giới thiệu](#-giới-thiệu)
- [✨ Tính năng](#-tính-năng)
- [🏗️ Kiến trúc](#️-kiến-trúc)
- [🛠️ Công nghệ](#️-công-nghệ)
- [📦 Cài đặt](#-cài-đặt)
- [🚀 Khởi chạy](#-khởi-chạy)
- [📖 API Documentation](#-api-documentation)
- [🎨 Giao diện](#-giao-diện)
- [🔧 Cấu hình](#-cấu-hình)
- [📱 Tính năng Thanh toán](#-tính-năng-thanh-toán)
- [🎉 Hệ thống Khuyến mãi](#-hệ-thống-khuyến-mãi)
- [🧪 Testing](#-testing)
- [📚 Tài liệu](#-tài-liệu)
- [🤝 Đóng góp](#-đóng-góp)

## 🎯 Giới thiệu

**ShoeStore** là một hệ thống thương mại điện tử hoàn chỉnh được thiết kế đặc biệt cho cửa hàng giày dép. Dự án sử dụng kiến trúc **Full-stack** với:

- **Backend**: ASP.NET Core 9.0 + Blazor Server (Admin Panel)
- **Frontend**: ReactJS 18 + Shadcn UI (Customer Interface) 
- **Database**: SQL Server với Entity Framework Core
- **Authentication**: JWT cho API, Cookie cho Admin

## ✨ Tính năng

### 🛍️ **Khách hàng (Frontend)**
- **🔐 Xác thực**: Đăng ký, đăng nhập, quên mật khẩu, đổi mật khẩu
- **👤 Quản lý tài khoản**: Thông tin cá nhân, lịch sử đơn hàng
- **🔍 Sản phẩm**: Tìm kiếm, lọc theo thương hiệu/danh mục/giá/size
- **🛒 Giỏ hàng**: Thêm/xóa/cập nhật sản phẩm, tính toán tự động
- **🎫 Voucher**: Áp dụng mã giảm giá, lịch sử sử dụng
- **🎉 Khuyến mãi**: Hiển thị giá khuyến mãi tự động
- **💳 Thanh toán**: MoMo, VnPay, tích hợp gateway
- **📦 Đơn hàng**: Theo dõi trạng thái, chi tiết đơn hàng

### 🏢 **Quản trị (Admin Panel)**
- **📊 Dashboard**: Thống kê doanh thu, phân tích
- **👥 Quản lý người dùng**: Danh sách, phân quyền
- **📦 Quản lý sản phẩm**: CRUD sản phẩm, hình ảnh, kho hàng
- **🏷️ Thương hiệu & Danh mục**: Quản lý phân loại
- **📋 Quản lý đơn hàng**: Xử lý, cập nhật trạng thái
- **🎫 Quản lý Voucher**: Tạo, cập nhật mã giảm giá
- **🎉 Quản lý Khuyến mãi**: Tạo chương trình khuyến mãi

### 💻 **API Features**
- **🔒 JWT Authentication**: Bảo mật API endpoints
- **📱 RESTful APIs**: Chuẩn REST cho frontend
- **📄 Swagger Documentation**: API documentation tự động
- **🚀 Performance**: Caching, optimization
- **🔄 Real-time**: SignalR cho notifications

## 🏗️ Kiến trúc

```
📁 ShoeStore/
├── 🌐 WebApp/                 # Backend ASP.NET Core
│   ├── 🎮 Controllers/        # API Controllers
│   ├── 🔧 Services/           # Business Logic
│   ├── 📊 Data/               # Entity Framework
│   ├── 📝 Models/             # Entities & DTOs
│   ├── 🎨 BlazorPages/        # Admin UI
│   └── 🌍 wwwroot/            # Static files
├── ⚛️ shoestore-react/        # Frontend React
│   ├── 🎨 src/components/     # UI Components
│   ├── 📄 src/pages/          # Page Components
│   ├── 🔧 src/services/       # API Services
│   └── 🎯 src/context/        # State Management
├── 🗄️ sqlscripts/            # Database Scripts
├── 📋 Plan/                   # Planning Documents
└── 🧪 WebApp.Tests/           # Unit Tests
```

## 🛠️ Công nghệ

### **Backend Stack**
- **Framework**: ASP.NET Core 9.0
- **UI Admin**: Blazor Server + AntDesign 1.4.0
- **Database**: SQL Server + Entity Framework Core 9.0
- **Authentication**: JWT Bearer + ASP.NET Identity
- **Caching**: Memory Cache / Redis
- **File Storage**: Imgur API
- **Email**: SMTP Gmail
- **Documentation**: Swagger/OpenAPI

### **Frontend Stack**
- **Framework**: React 18.2.0
- **Routing**: React Router DOM 7.6.0
- **UI Library**: Shadcn UI + Radix UI
- **Styling**: Tailwind CSS 3.4.1
- **Animation**: Framer Motion 12.15.0
- **HTTP Client**: Axios 1.9.0
- **Icons**: Lucide React + Radix Icons
- **Build Tool**: Vite 5.1.0

### **Payment Integration**
- **MoMo**: E-wallet payment gateway
- **VnPay**: Bank payment gateway
- **Security**: HMAC-SHA256 encryption

## 📦 Cài đặt

### **Yêu cầu hệ thống**
- **.NET SDK**: 9.0 trở lên
- **Node.js**: 18.0 trở lên
- **SQL Server**: 2019 trở lên
- **Visual Studio**: 2022 (khuyến nghị)

### **1. Clone Repository**
```bash
git clone https://github.com/your-username/ShoeStore.git
cd ShoeStore
```

### **2. Setup Database**
```bash
# Tạo database từ script
sqlcmd -S . -i sqlscripts/db.sql

# Hoặc sử dụng Migration
cd WebApp
dotnet ef database update
```

### **3. Cấu hình Backend**
```bash
cd WebApp

# Cập nhật connection string trong appsettings.json
# Cấu hình email, payment gateway settings

# Restore packages
dotnet restore
```

### **4. Cấu hình Frontend**
```bash
cd shoestore-react

# Install dependencies
npm install

# Tạo file .env.local (optional)
echo "VITE_API_URL=https://localhost:5001" > .env.local
```

## 🚀 Khởi chạy

### **Development Mode**

**Terminal 1 - Backend:**
```bash
cd WebApp
dotnet run
# Backend sẽ chạy tại: https://localhost:5001
# Admin Panel: https://localhost:5001/admin
```

**Terminal 2 - Frontend:**
```bash
cd shoestore-react
npm run dev
# Frontend sẽ chạy tại: https://localhost:5001
```

### **Production Build**

**Backend:**
```bash
cd WebApp
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
cd shoestore-react
npm run build
# Build files sẽ được tạo trong thư mục dist/
```

## 📖 API Documentation

### **Swagger UI**
- **URL**: `https://localhost:5001/swagger`
- **Environment**: Development mode

### **Main API Endpoints**

#### **🔐 Authentication**
```http
POST /api/auth/login          # Đăng nhập
POST /api/auth/register       # Đăng ký
POST /api/auth/refresh        # Refresh token
POST /api/auth/logout         # Đăng xuất
POST /api/auth/forgot-password # Quên mật khẩu
POST /api/auth/reset-password  # Đặt lại mật khẩu
```

#### **👤 Account Management**
```http
GET    /api/account/profile   # Thông tin tài khoản
PUT    /api/account/profile   # Cập nhật thông tin
POST   /api/account/change-password # Đổi mật khẩu
```

#### **📦 Products**
```http
GET    /api/products          # Danh sách sản phẩm
GET    /api/products/{id}     # Chi tiết sản phẩm
GET    /api/products/filter   # Lọc sản phẩm
GET    /api/products/search   # Tìm kiếm sản phẩm
```

#### **🛒 Cart**
```http
GET    /api/cart              # Lấy giỏ hàng
POST   /api/cart/items        # Thêm sản phẩm
PUT    /api/cart/items/{id}   # Cập nhật số lượng
DELETE /api/cart/items/{id}   # Xóa sản phẩm
```

#### **📋 Orders**
```http
GET    /api/orders            # Danh sách đơn hàng
GET    /api/orders/{id}       # Chi tiết đơn hàng
POST   /api/orders            # Tạo đơn hàng
PUT    /api/orders/{id}/status # Cập nhật trạng thái
```

#### **🎫 Vouchers**
```http
GET    /api/vouchers          # Danh sách voucher khả dụng
POST   /api/vouchers/validate # Kiểm tra mã voucher
GET    /api/vouchers/history  # Lịch sử sử dụng
```

#### **💳 Payment**
```http
POST   /api/payment/momo      # Thanh toán MoMo
POST   /api/payment/vnpay     # Thanh toán VnPay
GET    /api/payment/momo-return # MoMo callback
GET    /api/payment/vnpay-return # VnPay callback
```

## 🎨 Giao diện

### **🛍️ Customer Interface (React)**
- **Design System**: Shadcn UI + Tailwind CSS
- **Theme**: Modern, minimal, responsive
- **Components**: 
  - Header with navigation & cart
  - Product cards with promotion badges
  - Cart drawer with real-time updates
  - Checkout flow with payment integration
  - Order tracking interface

### **🏢 Admin Panel (Blazor)**
- **Design System**: AntDesign 1.4.0
- **Layout**: Sidebar navigation + main content
- **Features**:
  - Dashboard với charts & statistics
  - Data tables với pagination & sorting
  - Form validation & file uploads
  - Real-time notifications

## 🔧 Cấu hình

### **Database Connection**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=ShoeStore;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### **JWT Settings**
```json
{
  "JwtSettings": {
    "SecretKey": "Your-Secret-Key-Min-32-Chars",
    "Issuer": "ShoeStore",
    "Audience": "ShoeStoreClient",
    "ExpiryInMinutes": 60,
    "RefreshTokenExpiryInDays": 7
  }
}
```

### **Email Configuration**
```json
{
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

## 📱 Tính năng Thanh toán

### **🎯 Payment Gateways**

#### **MoMo E-Wallet**
- **Features**: QR Code, App-to-app payment
- **Security**: HMAC-SHA256 signature
- **Test Environment**: Sandbox mode
- **Supported**: iOS, Android, Web

#### **VnPay Gateway**
- **Features**: ATM Cards, Internet Banking
- **Security**: SHA-256 hash validation
- **Banks**: Vietcombank, VietinBank, BIDV, v.v.
- **Test Environment**: Sandbox mode

### **💰 Payment Flow**
1. **Chọn phương thức** thanh toán
2. **Redirect** đến gateway (MoMo/VnPay)
3. **Xử lý thanh toán** trên gateway
4. **Callback** về hệ thống
5. **Cập nhật** trạng thái đơn hàng
6. **Thông báo** kết quả cho khách hàng

## 🎉 Hệ thống Khuyến mãi

### **✨ Promotion Features**
- **🎯 Target Types**: Product, Category, Brand, Global
- **💰 Discount Types**: Percentage (%), Fixed Amount (VND)
- **⏰ Time-based**: Start Date, End Date
- **🔢 Priority System**: Áp dụng promotion có priority cao nhất
- **🚀 Auto-calculation**: Tính toán tự động trong cart & product

### **🎨 Visual Elements**
- **Badge Design**: Purple gradient promotion badges
- **Price Display**: ~~Original Price~~ **Promotion Price**
- **Cart Integration**: Real-time promotion calculation
- **Responsive**: Mobile-friendly design

### **📊 Sample Promotions**
1. **Flash Sale 25%** - Giảm 25% toàn bộ sản phẩm
2. **Sneaker Sale 15%** - Giảm 15% giày sneaker
3. **Nike Sale 20%** - Giảm 20% thương hiệu Nike
4. **Fixed 200K Off** - Giảm 200,000 VND cố định
5. **VIP 30%** - Giảm 30% (priority cao)

## 🧪 Testing

### **Unit Tests**
```bash
cd WebApp.Tests
dotnet test
```

### **Test Coverage**
- ✅ **Services**: BrandService, CategoryService, ProductService
- ✅ **Controllers**: BrandsController, CategoriesController, ProductsController
- ✅ **Authentication**: AuthService testing
- ✅ **Data Layer**: Entity mappings & validations

### **API Testing**
```bash
# Test promotion system
curl -X GET "https://localhost:5001/api/tests/active-promotions"

# Test product with promotion
curl -X GET "https://localhost:5001/api/tests/promotion-price/product-id"
```

## 📚 Tài liệu

### **📋 Planning Documents**
- `Plan/Promotion.Plan.md` - Kế hoạch hệ thống khuyến mãi
- `Plan/Cart.Plan.md` - Kế hoạch giỏ hàng
- `Plan/Order.Plan.md` - Kế hoạch đơn hàng
- `Plan/Voucher.Plan.md` - Kế hoạch voucher
- `Plan/eWalletPayment.Plan.md` - Kế hoạch thanh toán

### **📖 Feature Documentation**
- `PROMOTION_IMPLEMENTATION_SUMMARY.md` - Tổng kết hệ thống khuyến mãi
- `WebApp/Services/Promotions/README.md` - Documentation chi tiết
- `WebApp/Services/Vouchers/README.md` - Voucher system guide

### **🔌 API Documentation**
- `WebApp/Services/Promotions/PromotionAPI_Documentation.md`
- `WebApp/Services/Vouchers/Phase3_API_Documentation.md`

## 🤝 Đóng góp

### **Development Workflow**
1. **Fork** repository
2. **Create** feature branch: `git checkout -b feature/ten-tinh-nang`
3. **Commit** changes: `git commit -m 'Add: tính năng mới'`
4. **Push** branch: `git push origin feature/ten-tinh-nang`
5. **Create** Pull Request

### **Code Standards**
- **C# Conventions**: PascalCase cho classes/methods, camelCase cho variables
- **JavaScript Conventions**: camelCase cho functions/variables
- **Git Commit**: `Add:`, `Fix:`, `Update:`, `Remove:` prefix
- **Documentation**: Update README cho features mới

### **Project Structure Rules**
- **Backend**: Feature-based folders trong `Services/`, `Controllers/`
- **Frontend**: Component-based structure trong `src/components/`
- **Database**: Migrations trong `Data/Migrations/`
- **Testing**: Mirror structure trong `WebApp.Tests/`

---

<div align="center">

**🏆 ShoeStore - Hệ thống thương mại điện tử chuyên nghiệp**

Made with ❤️ by ShoeStore Team

[![GitHub](https://img.shields.io/badge/GitHub-ShoeStore-181717)](https://github.com/your-username/ShoeStore)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

</div>
