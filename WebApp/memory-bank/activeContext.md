# Active Context
- Xây dựng chức năng đặt hàng và quản lý đơn hàng.
- Thanh toán sau COD hoặc qua cổng thanh toán MoMo.
- Cho phép khách vãng lai mua ngay hoặc checkout giỏ hàng.

## Current State
- Backend đã hoàn thiện các module cơ bản: sản phẩm, danh mục, thương hiệu, kho, giỏ hàng.
- Đã tích hợp MemoryCache, lưu trữ ảnh Imgur.
- Đang phát triển: Các module: giỏ hàng, thanh toán, đơn hàng, tích hợp MoMo.
- Tiếp theo: các module: mã giảm giá, chương trình khuyến mãi.

## Previous Context
### Cart Module
- Tạo Các Dto: CartDto, CartItem
- Lưu thông tin cart trong cache
- Tạo CartService
- Tạo CartsController
- Dùng guestId cho khách vãng lai, guestId được FE generate và gắn vào header của request

## Issues