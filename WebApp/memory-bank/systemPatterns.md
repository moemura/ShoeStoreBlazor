# System Patterns

> Tham chiếu quy tắc kiến trúc, best practices tại `.cursor/rules/solution.rules.mdc` và các file rules liên quan. Chỉ ghi nhận các quyết định, trạng thái, hoặc ngoại lệ thực tế tại đây.

## Architecture
- Đã áp dụng: ASP.NET Core + Blazor Server (InteractiveServerRenderMode), chia theo feature folder, EF Core cho CRUD.
- Admin UI (Blazor page) gọi trực tiếp service (theo rule).

## Design Principles
- Đã áp dụng DI cho tất cả service, JWT cho API, Cookie cho admin (theo rule).
- Nếu có ngoại lệ hoặc thay đổi, sẽ ghi rõ tại đây. 