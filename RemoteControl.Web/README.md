# Remote-Desktop-Web-App

Ứng dụng theo dõi nhiều máy tính từ xa qua socket, xây dựng bằng Blazor (ASP.NET Core) cho frontend. Backend sẽ được bổ sung sau.

## Mục đích của README

Hướng dẫn nhanh cách clone dự án về máy, cài đặt phụ thuộc và chạy môi trường phát triển (development).

## Yêu cầu (Prerequisites)

- .NET SDK tương thích với `TargetFramework` của dự án (dự án hiện tại target: `net10.0`). Kiểm tra bằng:

```powershell
dotnet --list-sdks
```

- Node.js (kèm npm) để chạy Tailwind CSS build/watch. (node.js: v22.20.0 | npm: 11.6.2)
```
node --version
npm --version
```
- Git để clone repository.

## Clone repository

1. Clone repo (thay `<repository-url>` bằng URL thực tế):

```powershell
git clone <repository-url>
```

2. Chuyển vào thư mục dự án (tên thư mục tùy vào repo):

```powershell
cd "Blazor Web App Learning"
```

## Cài đặt phụ thuộc

1. Cài .NET dependencies:

```powershell
dotnet restore
```

2. Cài Node dependencies (để dùng Tailwind):

```powershell
npm install
```

## Chạy ứng dụng (môi trường phát triển)

1. Khởi chạy Tailwind watch (từ thư mục gốc nơi có `package.json`):

```powershell
npm run watch
```

Lệnh này sẽ biên dịch `Styles/app.css` thành `wwwroot/css/app.css` và theo dõi thay đổi.

2. Chạy ứng dụng Blazor:

```powershell
dotnet watch
```

Hoặc, nếu bạn muốn chỉ định project file:

```powershell
dotnet run --project "Blazor Web App Learning.csproj"
```

Sau khi chạy thành công, ứng dụng thường mở tại `https://localhost:5001` hoặc URL được in ra trong terminal.

## Lưu ý về versioning và file được ignore

- File `wwwroot/css/app.css` hiện được ignore trong `.gitignore` — đó là file CSS được biên dịch từ Tailwind. Nếu bạn muốn commit file CSS đã biên dịch (ví dụ cho môi trường production), chỉnh `.gitignore` tương ứng.

## Cấu trúc thư mục chính

- `Components/` — component Blazor
- `Pages/` — các trang Razor
- `Styles/` — nguồn Tailwind (ví dụ `Styles/app.css`)
- `wwwroot/` — các tài sản tĩnh (CSS biên dịch, JS, images)

## Ghi chú thêm

- Backend socket và các chi tiết kết nối sẽ được bổ sung sau; README sẽ được cập nhật khi backend có mặt.
- Nếu gặp lỗi về SDK, hãy cài đúng phiên bản .NET tương ứng hoặc chỉnh `TargetFramework` trong `.csproj` nếu muốn dùng SDK cũ hơn.
