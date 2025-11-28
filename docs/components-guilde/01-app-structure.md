# Cấu trúc Ứng dụng (App Structure)

Tài liệu này giải thích chi tiết về các tệp tin cốt lõi cấu thành nên ứng dụng Blazor này. Hiểu rõ các tệp này sẽ giúp bạn nắm bắt được luồng hoạt động của ứng dụng.

## 1. `Program.cs`

Đây là "cửa ngõ" của ứng dụng. Mọi thứ bắt đầu từ đây.

*   **Chức năng chính**:
    *   Khởi tạo `WebApplicationBuilder`.
    *   Đăng ký các dịch vụ (Services) vào Container (ví dụ: `builder.Services.AddRazorComponents()`).
    *   Cấu hình HTTP request pipeline (ví dụ: `app.UseHttpsRedirection()`, `app.UseStaticFiles()`).
    *   Định nghĩa component gốc để render: `app.MapRazorComponents<App>()`.

## 2. `Components/App.razor`

Đây là **Root Component** (Thành phần gốc). Nó đóng vai trò như bộ khung HTML cơ bản cho toàn bộ ứng dụng.

*   **Nội dung chính**:
    *   Cấu trúc HTML chuẩn: `<html>`, `<head>`, `<body>`.
    *   `<HeadOutlet />`: Cho phép các trang con thay đổi tiêu đề trang (`<title>`) hoặc thẻ meta.
    *   `<Routes />`: Nơi nội dung của các trang sẽ được hiển thị dựa trên đường dẫn URL.
    *   Nhúng các file CSS và Script toàn cục (ví dụ: `app.css`, `blazor.web.js`).

> **Lưu ý**: Trong file này có sử dụng `@rendermode="RenderMode.InteractiveServer"`. Điều này kích hoạt chế độ tương tác thời gian thực (Interactive Server) cho ứng dụng, cho phép xử lý sự kiện (như click nút) ngay lập tức.

## 3. `Components/Routes.razor`

Component này chịu trách nhiệm điều hướng (Routing). Nó quyết định xem sẽ hiển thị trang nào dựa trên URL hiện tại của trình duyệt.

*   **Thành phần quan trọng**:
    *   `<Router>`: Component quản lý việc định tuyến.
    *   `<Found>`: Được hiển thị khi tìm thấy trang khớp với URL. Bên trong nó sử dụng `<RouteView>` để hiển thị trang đó cùng với Layout mặc định (`MainLayout`).
    *   `<NotFound>`: Được hiển thị khi không tìm thấy trang nào khớp với URL (thường là trang báo lỗi 404).

## 4. `Components/_Imports.razor`

Đây là một tệp đặc biệt. Bạn không cần viết code logic ở đây.

*   **Chức năng**: Chứa các khai báo `using` (tương tự như `import` trong các ngôn ngữ khác).
*   **Tác dụng**: Các thư viện được khai báo ở đây sẽ có hiệu lực cho **tất cả** các component khác trong thư mục `Components`. Điều này giúp bạn không phải lặp lại việc khai báo `using` trong từng file `.razor` riêng lẻ.

Ví dụ:
```csharp
@using Microsoft.AspNetCore.Components.Web
@using Blazor_Web_App_Learning.Components
```
Nhờ có dòng trên, bạn có thể sử dụng các component trong thư mục `Components` ở bất kỳ đâu mà không cần gọi tên đầy đủ namespace.
