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
├── 🗄️ sqlscripts/             # Database Scripts
├── 📋 Plan/                   # Planning Documents
└── 🧪 WebApp.Tests/           # Unit Tests
```

## 🛠️ Công nghệ

### **Backend Stack**
- **Framework**: ASP.NET Core 9.0
- **UI Admin**: Blazor Server + AntDesign 1.4.0
- **Database**: SQL Server + Entity Framework Core 9.0
- **Authentication**: JWT Bearer/Cookie + ASP.NET Identity
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
git clone https://github.com/nghuuan2803/ShoeStore.git
```
### **2. Cấu hình chuỗi kết nối database**
```json
# appsettings.json
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=ShoeStore;Trusted_Connection=True;TrustServerCertificate=True;"
```
### **3. Tạo Database**
```bash
# Sử dụng Migration
cd ShoeStore
cd WebApp
dotnet ef database update
```


```bash
# Restore packages
dotnet restore
```

### **4. Cấu hình Frontend**
```bash
cd shoestore-react

# Install dependencies
npm install
```

## 🚀 Khởi chạy

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
# Frontend sẽ chạy tại: http://localhost:3000
```

## 🎨 Giao diện

### **🛍️ Customer Interface (React)**
- **Design**: Shadcn UI + Tailwind CSS
- **Theme**: Modern, minimal, responsive
- **Components**: 
  - Header with navigation & cart
  - Product cards with promotion badges
  - Cart drawer with real-time updates
  - Checkout flow with payment integration
  - Order tracking interface

### **🏢 Admin Panel (Blazor WebApp)**
- **Design System**: AntDesign 1.4.0
- **Layout**: Sidebar navigation + main content
- **Features**:
  - Dashboard với charts & statistics
  - Data tables với pagination & sorting
  - Form validation & file uploads
  - Real-time notifications

## 📱 Tính năng Thanh toán

### **🎯 Payment Gateways**

#### **MoMo E-Wallet**
- **Features**: QR Code, App-to-app payment
- **Security**: HMAC-SHA256 signature
- **Test Environment**: Sandbox mode

#### **VnPay Gateway**
- **Features**: ATM Cards, Internet Banking
- **Security**: SHA-256 hash validation
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


---

<div align="center">

Made with ❤️ by annghdev

</div>
