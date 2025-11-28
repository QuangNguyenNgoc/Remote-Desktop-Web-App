# Bố cục (Layouts)

Tài liệu này giải thích về cách tổ chức bố cục (Layout) trong ứng dụng. Layout giúp định nghĩa các phần giao diện chung (như thanh menu, tiêu đề) xuất hiện trên nhiều trang khác nhau.

## 1. `MainLayout.razor`

Đây là bố cục chính của ứng dụng, được áp dụng mặc định cho hầu hết các trang (được cấu hình trong `Routes.razor`).

*   **Cấu trúc**:
    *   Sử dụng Flexbox để chia màn hình thành 2 phần: Sidebar (trái) và Main Content (phải).
    *   **Sidebar**: Chứa menu điều hướng.
    *   **TopBar**: Thanh tiêu đề ở trên cùng của phần nội dung chính.
    *   **`@Body`**: Đây là nơi nội dung của từng trang (Page) cụ thể sẽ được hiển thị.
*   **Code logic**:
    *   `public static string PageTitle`: Một biến tĩnh (static) dùng để lưu tiêu đề trang hiện tại. Các trang con có thể gán giá trị cho biến này để thay đổi tiêu đề trên TopBar.

## 2. `Sidebar.razor`

Thanh bên trái của ứng dụng.

*   **Thành phần**:
    *   Logo và tên ứng dụng (NETSPY).
    *   **`<NavMenu />`**: Component chứa các liên kết điều hướng.
    *   Thông tin người dùng (Admin User) ở dưới cùng.

## 3. `NavMenu.razor`

Menu điều hướng chính nằm bên trong Sidebar.

*   **Thành phần**:
    *   Sử dụng component `NavItem` (được định nghĩa trong Shared) để tạo từng mục menu.
    *   Mỗi `NavItem` có thuộc tính `Href` (đường dẫn) và `Text` (tên hiển thị).
    *   `Match="NavLinkMatch.All"`: Chỉ định rằng mục này chỉ active khi URL khớp hoàn toàn (thường dùng cho trang chủ `/`).

## 4. `TopBar.razor`

Thanh tiêu đề nằm trên cùng của khu vực nội dung chính.

*   **Chức năng**:
    *   Hiển thị tiêu đề của trang hiện tại (`PageTitle`).
    *   Hiển thị các nút chức năng chung (ví dụ: thông báo).
*   **Code logic**:
    *   `[Parameter] public string PageTitle`: Nhận tiêu đề từ `MainLayout` truyền vào.
