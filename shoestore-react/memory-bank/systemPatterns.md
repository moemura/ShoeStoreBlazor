# System Patterns

> Tham chiếu quy tắc kiến trúc, best practices tại `.cursor/rules/solution.rules.mdc` và các file rules liên quan. Chỉ ghi nhận các quyết định, trạng thái, hoặc ngoại lệ thực tế tại đây.

## Architecture
- Đã áp dụng: ReactJS 18, chia theo feature/folder, Shadcn UI, gọi API backend qua services.
- Quản lý state cục bộ (useState, useContext), sẽ cân nhắc mở rộng Redux nếu phức tạp hơn.

## Design Principles
- Đã tách biệt rõ UI, logic, gọi API (theo rule).
- Nếu có ngoại lệ hoặc thay đổi, sẽ ghi rõ tại đây.

## Additional Notes
- Không sử dụng TypeScript 