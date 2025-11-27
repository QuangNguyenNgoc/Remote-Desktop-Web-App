# Các Trang (Pages)

Tài liệu này giải thích chi tiết về các trang (Pages) trong thư mục `Components/Pages`. Mỗi trang tương ứng với một đường dẫn (URL) cụ thể trong ứng dụng.

## 1. Trang Chủ (`Home.razor`)

*   **Đường dẫn**: `/` (Trang mặc định).
*   **Chức năng**: Hiển thị bảng điều khiển (Dashboard) tổng quan về hệ thống.
*   **Thành phần chính**:
    *   **Thẻ thống kê (`StatCard`)**: Hiển thị các số liệu như số lượng Agent đang hoạt động, tổng số Keylog, v.v.
    *   **Bảng hoạt động (`RecentActivities`)**: Danh sách các hoạt động gần đây của các Agent.
*   **Code logic**:
    *   `OnInitialized()`: Khởi tạo dữ liệu giả lập cho danh sách hoạt động.
    *   `AgentActivityModel`: Class định nghĩa cấu trúc dữ liệu cho một dòng nhật ký hoạt động.

## 2. Quản lý Thiết bị (`DeviceManager.razor`)

*   **Đường dẫn**: `/devices`.
*   **Chức năng**: Hiển thị danh sách tất cả các thiết bị đang được quản lý.
*   **Thành phần chính**:
    *   **`SearchInput`**: Thanh tìm kiếm (hiện tại là giao diện).
    *   **`DeviceCard`**: Thẻ hiển thị thông tin tóm tắt của từng thiết bị (Tên, IP, OS, Trạng thái).
*   **Code logic**:
    *   Danh sách `Devices`: Chứa dữ liệu giả lập các thiết bị.
    *   Hàm `Connect(string name)`: Điều hướng người dùng đến trang chi tiết của thiết bị đó (sử dụng `NavigationManager`).

## 3. Điều khiển Thiết bị (`DeviceControl.razor`)

*   **Đường dẫn**: `/devices/{Id}` (Ví dụ: `/devices/HR-ADMIN-01`).
*   **Chức năng**: Trang chi tiết để điều khiển một thiết bị cụ thể.
*   **Thành phần chính**:
    *   **`DeviceHeader`**: Thanh thông tin trên cùng, hiển thị ID thiết bị và các nút thao tác nhanh.
    *   **`RemoteScreen`**: Khu vực hiển thị màn hình điều khiển từ xa.
    *   **`TerminalLog`**: Khu vực hiển thị nhật ký lệnh (Terminal).
*   **Code logic**:
    *   `[Parameter] public string Id`: Nhận giá trị `{Id}` từ URL để biết đang điều khiển thiết bị nào.

## 4. Các trang ví dụ mặc định

Dự án cũng bao gồm các trang mẫu mặc định của Blazor để bạn tham khảo:

*   **`Counter.razor` (`/counter`)**:
    *   Minh họa cách xử lý sự kiện click nút (`@onclick`).
    *   Biến `currentCount` tăng lên mỗi khi bấm nút và giao diện tự động cập nhật.
*   **`Weather.razor` (`/weather`)**:
    *   Minh họa cách hiển thị dữ liệu dạng bảng.
    *   Sử dụng `@attribute [StreamRendering]` để hiển thị dữ liệu dần dần (streaming) thay vì đợi tải xong toàn bộ.

## 5. Các trang tiện ích

*   **`Error.razor`**: Trang hiển thị khi có lỗi xảy ra trong ứng dụng.
*   **`NotFound.razor`**: Trang hiển thị khi người dùng truy cập vào một đường dẫn không tồn tại (Lỗi 404).
