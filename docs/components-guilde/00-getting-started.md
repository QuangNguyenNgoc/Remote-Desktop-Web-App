# Hướng dẫn Bắt đầu (Getting Started)

Chào mừng bạn đến với tài liệu hướng dẫn phát triển dự án **Remote Control Desktop**. Tài liệu này được thiết kế dành cho những người mới bắt đầu làm quen với Blazor và cấu trúc dự án này.

## 1. Yêu cầu hệ thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:

*   **Visual Studio Code** (hoặc Visual Studio 2022): Trình soạn thảo mã nguồn (IDE).
*   **.NET 8.0 SDK**: Bộ công cụ phát triển phần mềm cần thiết để chạy ứng dụng Blazor.

## 2. Cấu trúc dự án (Project Structure)

Dưới đây là cái nhìn tổng quan về các thư mục và tệp tin quan trọng trong dự án:

*   **`Program.cs`**: Điểm bắt đầu của ứng dụng. Nơi cấu hình các dịch vụ và pipeline xử lý request.
*   **`Components/`**: Thư mục chứa toàn bộ các thành phần giao diện (UI) của ứng dụng.
    *   **`Pages/`**: Chứa các trang web (ví dụ: Trang chủ, Trang quản lý thiết bị).
    *   **`Layout/`**: Chứa các bố cục chung (ví dụ: Thanh điều hướng, Sidebar).
    *   **`Shared/`**: Chứa các thành phần tái sử dụng (ví dụ: Thẻ thiết bị, Nút bấm).
    *   **`App.razor`**: Thành phần gốc (Root component) của ứng dụng.
    *   **`Routes.razor`**: Định nghĩa việc điều hướng (routing) trong ứng dụng.
*   **`wwwroot/`**: Chứa các tài nguyên tĩnh như hình ảnh, CSS, JavaScript.
*   **`appsettings.json`**: Tệp cấu hình của ứng dụng (ví dụ: chuỗi kết nối database).

## 3. Cách chạy dự án

Để chạy dự án và xem kết quả trên trình duyệt, bạn có thể sử dụng terminal trong VS Code:

1.  Mở Terminal (`Ctrl + ` ` `).
2.  Gõ lệnh sau và nhấn Enter:

```bash
dotnet watch
```

Lệnh `dotnet watch` sẽ khởi động ứng dụng và tự động tải lại trang (hot reload) mỗi khi bạn thay đổi mã nguồn, giúp việc phát triển nhanh chóng hơn.

## 4. Các khái niệm cơ bản cần biết

*   **Component (.razor)**: Một thành phần giao diện, có thể là một trang hoặc một phần nhỏ của trang.
*   **Razor Syntax**: Sự kết hợp giữa HTML và C# để tạo ra giao diện động. Bạn sẽ thấy các ký hiệu `@` được dùng để viết code C# ngay trong HTML.
