# Các Thành phần Dùng chung (Shared Components)

Tài liệu này giải thích về các component tái sử dụng (Reusable Components) trong thư mục `Components/Shared`. Các component này được thiết kế để có thể dùng ở nhiều nơi trong ứng dụng.

## 1. `DeviceCard.razor`

Thẻ hiển thị thông tin tóm tắt của một thiết bị.

*   **Sử dụng**: Trong trang `DeviceManager.razor`.
*   **Tham số (Parameters)**:
    *   `Name`: Tên thiết bị.
    *   `Ip`: Địa chỉ IP.
    *   `OS`: Hệ điều hành.
    *   `IsOnline`: Trạng thái kết nối (true/false).
    *   `LastSeen`: Thời gian hoạt động gần nhất.
    *   `OnConnect`: Sự kiện (EventCallback) được kích hoạt khi bấm nút "Connect".

## 2. `StatCard.razor`

Thẻ hiển thị số liệu thống kê trên Dashboard.

*   **Sử dụng**: Trong trang `Home.razor`.
*   **Tham số**:
    *   `Title`: Tiêu đề thẻ (ví dụ: "Active Agents").
    *   `Value`: Giá trị hiển thị chính (ví dụ: "12").
    *   `SubText`: Dòng chú thích nhỏ bên dưới.
    *   `Icon`: Đoạn mã HTML/SVG cho icon.
    *   `ColorClass`: Lớp CSS để định màu sắc cho icon (ví dụ: `bg-blue-500/10 text-blue-500`).

## 3. `NavItem.razor`

Mục menu điều hướng, dùng để tạo các liên kết trong Sidebar.

*   **Sử dụng**: Trong `NavMenu.razor`.
*   **Tham số**:
    *   `Href`: Đường dẫn đích.
    *   `Text`: Tên hiển thị của menu.
    *   `Icon`: Icon hiển thị bên cạnh tên.
    *   `Match`: Kiểu so khớp URL (ví dụ: `NavLinkMatch.All` cho trang chủ).

## 4. `SearchInput.razor`

Thanh tìm kiếm chung.

*   **Sử dụng**: Trong `DeviceManager.razor`.
*   **Tham số**:
    *   `Placeholder`: Dòng chữ gợi ý khi chưa nhập gì.
    *   `Value`: Giá trị hiện tại của ô nhập liệu (hỗ trợ binding 2 chiều).
    *   `OnKeyUp`: Sự kiện bắt phím bấm (ví dụ: để xử lý khi nhấn Enter).

## 5. `DeviceHeader.razor`

Thanh tiêu đề chi tiết cho trang điều khiển thiết bị.

*   **Sử dụng**: Trong `DeviceControl.razor`.
*   **Tham số**:
    *   `DeviceId`: ID của thiết bị đang điều khiển.
    *   `IpAddress`: IP của thiết bị.

## 6. Các component giao diện khác

*   **`RemoteScreen.razor`**: Giả lập màn hình điều khiển từ xa (hiện tại chỉ là hình ảnh tĩnh).
*   **`TerminalLog.razor`**: Giả lập khung hiển thị nhật ký lệnh (Terminal).

---

### Lời khuyên khi sử dụng Shared Components

Khi bạn muốn tạo một phần giao diện mới mà dự định sẽ dùng lại ở ít nhất 2 nơi, hãy cân nhắc tạo nó thành một Shared Component. Điều này giúp code gọn gàng hơn và dễ bảo trì hơn.
