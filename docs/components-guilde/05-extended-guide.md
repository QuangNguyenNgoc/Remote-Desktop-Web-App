# Hướng dẫn Mở rộng & Đề xuất (Extended Guide)

Tài liệu này cung cấp các gợi ý học tập và mẹo phát triển để bạn và bạn bè có thể nắm bắt Blazor tốt hơn và mở rộng dự án này.

## 1. Lộ trình học tập đề xuất

Để làm chủ dự án này và Blazor, bạn nên tìm hiểu theo thứ tự sau:

1.  **C# Cơ bản**: Hiểu về Class, Property, List, LINQ.
2.  **Blazor Data Binding**: Cách hiển thị dữ liệu từ code ra HTML (`@variable`) và ngược lại (`@bind`).
3.  **Component Parameters**: Cách truyền dữ liệu từ cha xuống con (`[Parameter]`).
4.  **Event Handling**: Cách xử lý sự kiện click, change (`@onclick`, `@onchange`).
5.  **Blazor Lifecycle**: Hiểu `OnInitialized` (chạy khi component khởi tạo) và `OnAfterRender` (chạy sau khi vẽ xong).

## 2. Các tính năng nên thử phát triển thêm

Để luyện tập, bạn có thể thử thêm các tính năng sau vào dự án:

*   **Thêm trang "Settings"**: Tạo một trang mới để cài đặt giao diện (Dark/Light mode).
*   **Dữ liệu thật**: Thay vì dùng dữ liệu giả (hard-coded List), hãy thử tạo một Service để quản lý danh sách thiết bị (Thêm, Sửa, Xóa).
*   **Form Validation**: Thêm chức năng thêm thiết bị mới và kiểm tra lỗi nhập liệu (ví dụ: bắt buộc nhập tên).

## 3. Mẹo Debug (Sửa lỗi)

*   **Sử dụng `Console.WriteLine`**: Trong Blazor Server, kết quả sẽ hiện ở Terminal chạy `dotnet watch`.
*   **Hot Reload**: Khi chạy `dotnet watch`, hầu hết các thay đổi giao diện sẽ cập nhật ngay lập tức. Nếu sửa code C# logic phức tạp, đôi khi cần khởi động lại ứng dụng (`Ctrl + R` trong terminal).
*   **Trình duyệt**: Nhấn F12 để xem Console của trình duyệt nếu có lỗi JavaScript hoặc lỗi mạng.

## 4. Tài liệu tham khảo

*   [Trang chủ Blazor (Microsoft Docs)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
*   [MudBlazor](https://mudblazor.com/): Một thư viện giao diện đẹp cho Blazor (nếu muốn thay thế CSS thủ công).
